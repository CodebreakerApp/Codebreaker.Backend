global using CodeBreaker.Data;
global using CodeBreaker.Shared.Exceptions;

global using Microsoft.ApplicationInsights.Channel;
global using Microsoft.ApplicationInsights.Extensibility;
global using Microsoft.EntityFrameworkCore;

global using System.Diagnostics;
using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
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
using System.Configuration;
#endif

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using CodeBreaker.APIs.Factories.GameTypeFactories;
using Azure.Core;

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

// AppConfiguration
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

// ApplicationInsights
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>();

// Swagger/EndpointDocumentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ICodeBreakerContext, CodeBreakerContext>(options =>
{
    string accountEndpoint = builder.Configuration["ApiService:Cosmos:AccountEndpoint"]
        ?? throw new ConfigurationErrorsException("ApiService:Cosmos:AccountEndpoint configuration is not available");
    string databaseName = builder.Configuration["ApiService:Cosmos:DatabaseName"]
        ?? throw new ConfigurationErrorsException("ApiService:Cosmos:DatabaseName configuration is not availabile");
    options.UseCosmos(accountEndpoint, azureCredential, databaseName);
});

// EventHub
builder.Services.AddSingleton<EventHubProducerClient>(builder =>
{
    ApiServiceOptions options = builder.GetRequiredService<ApiServiceOptions>();
    return new(options.EventHub.FullyQualifiedNamespace, options.EventHub.Name, azureCredential);
});

// Cache
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IGameCache, GameCache>();

// Application Services
builder.Services.AddSingleton<IGameTypeFactoryMapper<string>, GameTypeFactoryMapper<string>>(x => new GameTypeFactoryMapper<string>().Initialize(
    new GameType6x4Factory(),
    new GameType6x4MiniFactory(),
    new GameType8x5Factory()
));

builder.Services.AddSingleton<IPublishEventService, EventService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IMoveService, MoveService>();

// CORS
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

builder.Services.AddRequestDecompression();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("standardLimiter", context =>
        RateLimitPartition.GetConcurrencyLimiter("standardLimiter", key => new ConcurrencyLimiterOptions
        {
            PermitLimit = 10,
            QueueLimit = 5,
            QueueProcessingOrder = QueueProcessingOrder.NewestFirst
        }));
});

var app = builder.Build();

app.UseCors(AllowCodeBreakerOrigins);
app.UseRequestDecompression();

app.UseRateLimiter();

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
.Produces<GetGamesResponse>(StatusCodes.Status200OK)
.WithName("GetGames")
.WithSummary("Get games by the given date")
.WithOpenApi(x =>
{
    x.Parameters[0].Description = "The of date to get the games from. (e.g. 2022-01-01)";
    return x;
})
.RequireRateLimiting("standardLimiter");

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
.Produces(StatusCodes.Status404NotFound)
.WithName("GetGame")
.WithSummary("Gets a game by the given id")
.WithOpenApi(x =>
{
    x.Parameters[0].Description = "The id of the game to get";
    return x;
})
.RequireRateLimiting("standardLimiter");

app.MapGet("/gametypes", (
    [FromServices] IGameTypeFactoryMapper<string> gameTypeFactoryMapper
) =>
{
    IEnumerable<GameType<string>> gameTypes = gameTypeFactoryMapper.GetAllFactories().Select(x => x.Create());
    return Results.Ok(new GetGameTypesResponse(gameTypes.Select(x => x.ToDto())));
})
.Produces<GetGameTypesResponse>(StatusCodes.Status200OK)
.WithName("GetGameTypes")
.WithSummary("Gets the available game-types")
.WithOpenApi()
.RequireRateLimiting("standardLimiter");

// Create game
app.MapPost("/games", async (
    [FromBody] CreateGameRequest req,
    [FromServices] IGameTypeFactoryMapper<string> gameTypeFactoryMapper,
    [FromServices] IGameService gameService) =>
{
    GameTypeFactory<string> gameTypeFactory;

    try
    {
        gameTypeFactory = gameTypeFactoryMapper[req.GameType];
    }
    catch (GameTypeNotFoundException)
    {
        return Results.BadRequest("Gametype does not exist");
    }
    
    Game game = await gameService.CreateAsync(req.Username, gameTypeFactory);

    using var activity = activitySource.StartActivity("Game started", ActivityKind.Server);
    gamesStarted.Add(1);
    activity?.AddBaggage("GameId", game.GameId.ToString());
    activity?.AddBaggage("Name", req.Username);
    activity?.AddEvent(new ActivityEvent("Game started"));

    return Results.Created($"/games/{game.GameId}", new CreateGameResponse(game.ToDto()));
})
.Produces<CreateGameResponse>(StatusCodes.Status201Created)
.WithName("CreateGame")
.WithSummary("Creates and starts a game")
.WithOpenApi(x =>
{
    x.RequestBody.Description = "The data of the game to create";
    return x;
})
.RequireRateLimiting("standardLimiter");

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
.Produces(StatusCodes.Status204NoContent)
.WithName("CancelOrDeleteGame")
.WithSummary("Cancels or deletes the game with the given id")
.WithDescription("A cancelled game remains in the database, whereas a deleted game does not.")
.WithOpenApi(x =>
{
    x.Parameters[0].Description = "The id of the game to delete or cancel";
    x.Parameters[1].Description = "Defines whether the game should be cancelled or deleted.";
    return x;
})
.RequireRateLimiting("standardLimiter");

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

    using var activity = activitySource.StartActivity("Game Move", ActivityKind.Server);
    activity?.AddBaggage("GameId", gameId.ToString());
    movesDone.Add(1);

    KeyPegs? keyPegs = game.GetLastKeyPegsOrDefault();

    if (keyPegs is null)
        return Results.BadRequest("Could not get keyPegs");

    return Results.Ok(new CreateMoveResponse(((KeyPegs)keyPegs).ToDto(), game.Ended, game.Won));
})
.Produces<CreateMoveResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.WithName("CreateMove")
.WithSummary("Creates a move for the game with the given id")
.WithOpenApi(x =>
{
    x.Parameters[0].Description = "The id of the game to create a move for";
    x.RequestBody.Description = "The data for creating the move";
    return x;
})
.RequireRateLimiting("standardLimiter");

app.Run();