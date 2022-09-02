global using CodeBreaker.APIs;
global using CodeBreaker.APIs.Data;
global using CodeBreaker.APIs.Exceptions;
global using CodeBreaker.APIs.Extensions;
global using CodeBreaker.Shared;

global using Microsoft.ApplicationInsights.Channel;
global using Microsoft.ApplicationInsights.Extensibility;
global using Microsoft.EntityFrameworkCore;

global using System.Collections.Concurrent;
global using System.Diagnostics;
using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using CodeBreaker.APIs.Data.Factories.GameTypeFactories;
using CodeBreaker.APIs.Options;
using CodeBreaker.APIs.Services;
using CodeBreaker.APIs.Services.Cache;
using CodeBreaker.APIs.Utilities;
using CodeBreaker.Shared.Models.Api;
using CodeBreaker.Shared.Models.Data;
using CodeBreaker.Shared.Models.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;


#if USEPROMETHEUS
using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
#endif

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CodeBreaker.APIs.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

ActivitySource activitySource = new("CNinnovation.CodeBreaker.API");
Meter meter = new("CodeBreaker.APIs", "1.0.0");
Counter<int> gamesStarted = meter.CreateCounter<int>("games-started", "games", "the number of games started");
Counter<int> movesDone = meter.CreateCounter<int>("moves-done", "moves", "the number of moves done");

#if USEPROMETHEUS
using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter("CodeBreaker.APIs")
    .AddPrometheusExporter(opt =>
    {
        opt.StartHttpListener = true;
        opt.HttpListenerPrefixes = new string[] { "http://localhost:9184/" };
    }).Build();
#endif

#if DEBUG
AzureCliCredential azureCredential = new();
#else
DefaultAzureCredential azureCredential = new();
#endif
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(options =>
{
    string endpoint = builder.Configuration["AzureAppConfigurationEndpoint"] ?? throw new ConfigurationErrorsException("AzureAppConfigurationEndpoint");
    options.Connect(new Uri(endpoint), azureCredential)
        .Select(KeyFilter.Any, LabelFilter.Null)
        .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
        .ConfigureKeyVault(vault => vault.SetCredential(azureCredential));
});
builder.Services.AddAzureAppConfiguration();
builder.Services.Configure<ApiServiceOptions>(builder.Configuration.GetSection("ApiService"));
builder.Services.AddSingleton(x => x.GetRequiredService<IOptions<ApiServiceOptions>>().Value);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationInsightsTelemetry(options => options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
builder.Services.AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ICodeBreakerContext, CodeBreakerContext>(options =>
{
    string connectionString = builder.Configuration
        .GetSection("CodeBreakerAPI")
        .GetConnectionString("CodeBreakerConnection")
        ?? throw new ConfigurationErrorsException("No connection string found with the configuration.");

    options.UseCosmos(connectionString, "codebreaker");
});

builder.Services.AddSingleton<EventHubProducerClient>(builder =>
{
    ApiServiceOptions options = builder.GetRequiredService<ApiServiceOptions>();
    return new(options.EventHub.FullyQualifiedNamespace, options.EventHub.Name, azureCredential);
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IGameCache, GameCache>();
builder.Services.AddSingleton<IGameTypeFactoryMapper<string>, GameTypeFactoryMapper<string>>(x => new GameTypeFactoryMapper<string>().Initialize());

builder.Services.AddSingleton<IPublishEventService, EventService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IMoveService, MoveService>();

const string AllowCodeBreakerOrigins = "_allowCodeBreakerOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowCodeBreakerOrigins,
        builder =>
        {
            builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors(AllowCodeBreakerOrigins);

app.UseSwagger();
app.UseSwaggerUI();

// -------------------------
// Endpoints
// -------------------------

app.MapGet("/games", (
    [FromQuery] DateTime date,
    [FromServices] IGameService gameService
) =>
{
    IAsyncEnumerable<GameDto> games = gameService
        .GetByDate(date)
        .Select(g => g.ToDto());
    return new GetGamesResponse(games.ToEnumerable());
})
.WithDescription("Get games by start-date")
.WithSummary("SummaryTest")
.WithName("NameTest")
.Produces<IEnumerable<GetGamesResponse>>(StatusCodes.Status200OK);

// Get game by id
app.MapGet("/games/{gameId:guid}", async (
    [FromRoute] Guid gameId,
    [FromServices] IGameService gameService
) =>
{
    Game? game = await gameService.GetAsync(gameId);

    if (game is null)
        return Results.NotFound();

    return Results.Ok(new GetGameResponse(game.ToDto()));
})
.Produces<GetGameResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// Create game
app.MapPost("/games", async (
    [FromBody] CreateGameRequest req,
    [FromServices] IGameTypeFactoryMapper<string> gameTypeFactoryMapper,
    [FromServices] IGameService gameService) =>
{
    GameTypeFactory<string> gameTypeFactory = gameTypeFactoryMapper[req.GameType];
    Game game = await gameService.CreateAsync(req.Username, gameTypeFactory);
    return Results.Created($"/games/{game.GameId}", new CreateGameResponse(game.ToDto()));
})
.Produces<CreateGameResponse>(StatusCodes.Status201Created);

// Cancel or delete game
app.MapDelete("/games/{gameId:guid}", async (
    [FromRoute] Guid gameId,
    [FromQuery] bool? cancel,
    [FromServices] IGameService gameService
) =>
{
    if (cancel == false)
        await gameService.DeleteAsync(gameId);
    else
        await gameService.CancelAsync(gameId);

    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent);

// Create move for game
app.MapPost("/games/{gameId:guid}/moves", async (
    [FromRoute] Guid gameId,
    [FromBody] CreateMoveRequest req,
    [FromServices] IMoveService moveService) =>
{
    Game game;
    Move move = new Move(0, req.GuessPegs.ToList(), null);

    try
    {
        game = await moveService.CreateMoveAsync(gameId, move);
    }
    catch (GameNotFoundException)
    {
        return Results.NotFound();
    }

    KeyPegs? keyPegs = game.GetLastKeyPegsOrDefault();

    if (keyPegs is null)
        return Results.BadRequest("Could not get keyPegs");

    return Results.Ok(new CreateMoveResponse((KeyPegs)keyPegs, game.Ended, game.Won));
})
.Produces<CreateMoveResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.Run();
