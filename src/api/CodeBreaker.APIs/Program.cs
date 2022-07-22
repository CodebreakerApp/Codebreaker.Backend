global using CodeBreaker.APIs;
global using CodeBreaker.APIs.Data;
global using CodeBreaker.APIs.Exceptions;
global using CodeBreaker.APIs.Extensions;
global using CodeBreaker.APIs.Services;
global using CodeBreaker.Shared;

global using Microsoft.ApplicationInsights.Channel;
global using Microsoft.ApplicationInsights.Extensibility;
global using Microsoft.EntityFrameworkCore;

global using System.Collections.Concurrent;
global using System.Diagnostics;
using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using CodeBreaker.APIs.Options;
using CodeBreaker.APIs.Utilities;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;


#if USEPROMETHEUS
using OpenTelemetry;
using OpenTelemetry.Metrics;
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

DefaultAzureCredential azureCredential = new();
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
    string connectionString = builder.Configuration["CodeBreakerAPI:ConnectionStrings:CodeBreakerConnection"] ?? throw new ConfigurationErrorsException("No connection string found with the configuration.");
    options.UseCosmos(connectionString, "codebreaker");
});
builder.Services.AddSingleton<EventHubProducerClient>(builder =>
{
    ApiServiceOptions options = builder.GetRequiredService<ApiServiceOptions>();
    return new(options.EventHub.FullyQualifiedNamespace, options.EventHub.Name, azureCredential);
});
builder.Services.AddSingleton<Game6x4Definition>();
builder.Services.AddSingleton<Game8x5Definition>();
builder.Services.AddSingleton<IGameCache, GameCache>();
builder.Services.AddSingleton<IPublishEventService, EventService>();
builder.Services.AddScoped<Game6x4Service>();
builder.Services.AddScoped<Game8x5Service>();
builder.Services.AddScoped<IGameAlgorithm, GameAlgorithm>();

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
app.UseAzureAppConfiguration();

app.MapPost("/start/{gameType}", async (CreateGameRequest request, string gameType, string? apiVersion) =>
{
    await using var scope = app.Services.CreateAsyncScope();

    IGameService? service = GetGameService(scope.ServiceProvider, gameType);
    if (service is null)
    {
        return Results.BadRequest("invalid game type");
    }

    using var activity = activitySource.StartActivity("Game started", ActivityKind.Server);
    gamesStarted.Add(1);

    Game game = await service.StartGameAsync(request.Name, GameTypes.Game6x4);
    activity?.AddBaggage("GameId", game.GameId.ToString());
    activity?.AddBaggage("Name", request.Name);
    activity?.AddEvent(new ActivityEvent("Game started"));

    return Results.Ok(new CreateGameResponse(game.GameId, new CreateGameOptions(GameTypes.Game6x4, Holes: game.Holes, MaxMoves: game.MaxMoves, game.ColorList.ToArray())));
}).WithDisplayName("PostStart")
.Produces<CreateGameResponse>(StatusCodes.Status200OK);

app.MapPost("/move/{gameType}", async (MoveRequest request, string gameType, string? apiVersion) =>
{
    try
    {
        // TODO: get game type from the game id, it should not be necessary to pass it with this request

        await using var scope = app.Services.CreateAsyncScope();

        IGameService? service = GetGameService(scope.ServiceProvider, gameType);
        if (service is null)
        {
            return Results.BadRequest("invalid game type");
        }

        using var activity = activitySource.StartActivity("Game Move", ActivityKind.Server);
        activity?.AddBaggage("GameId", request.Id.ToString());
        movesDone.Add(1);

        GameMove move = new(request.Id, request.MoveNumber, request.CodePegs.ToList());
        var result = await service.SetMoveAsync(move);
        MoveResponse response = new(result.GameId, result.Completed, result.Won, result.KeyPegs);
        return Results.Ok(response);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (GameException ex)
    {
        app.Logger.Error(ex, ex.Message);
        return Results.UnprocessableEntity(request);
    }
}).WithDisplayName("PostMove")
.Produces<MoveResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status422UnprocessableEntity)
.Produces(StatusCodes.Status400BadRequest);

app.MapGet("/report", async (CodeBreakerContext context, DateTime? date, string? apiVersion) =>
{
    DateTime definedDate = date ?? DateTime.Today;

    app.Logger.GameReport(definedDate.ToString("yyyy-MM-dd"));

    definedDate = definedDate.Date;

    var games = await context.GetGamesAsync(definedDate);
    return Results.Ok(games);
}).WithDisplayName("GetReport")
.Produces<IEnumerable<GamesInfo>>(StatusCodes.Status200OK);

app.MapGet("/reportdetail/{id}", async (CodeBreakerContext context, Guid id, string? apiVersion) =>
{
    app.Logger.DetailedGameReport(id);

    var games = await context.GetGameDetailAsync(id);
    return Results.Ok(games);
}).WithDisplayName("GetReportDetail")
.Produces<CodeBreakerGame>(StatusCodes.Status200OK);
app.Run();

IGameService? GetGameService(IServiceProvider provider, string gameType)
{
    if (gameType == "random")
    {
        gameType = new string[] { "6x4", "8x5" }[Random.Shared.Next(2)];
    }

    return gameType switch
    {
        "6x4" => provider.GetRequiredService<Game6x4Service>(),
        "8x5" => provider.GetRequiredService<Game8x5Service>(),
        _ => null
    };
}
