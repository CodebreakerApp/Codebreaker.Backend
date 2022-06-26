global using CodeBreaker.APIs;
global using CodeBreaker.APIs.Data;
global using CodeBreaker.APIs.Extensions;
global using CodeBreaker.APIs.Services;
global using CodeBreaker.Shared;
global using CodeBreaker.Shared.APIModels;

global using Microsoft.ApplicationInsights.Channel;
global using Microsoft.ApplicationInsights.Extensibility;
global using Microsoft.EntityFrameworkCore;

global using System.Collections.Concurrent;
global using System.Diagnostics;

#if USEPROMETHEUS
using OpenTelemetry;
using OpenTelemetry.Metrics;

using System.Configuration;
#endif

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CodeBreaker.APIs.Tests")]

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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
});
builder.Services.AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ICodeBreakerContext, CodeBreakerContext>(options =>
{
    string? connectionString = builder.Configuration.GetSection("CodeBreakerAPI").GetConnectionString("CodeBreakerConnection");
    if (connectionString is null) throw new ConfigurationErrorsException("No connection string found with the configuration.");
    options.UseCosmos(connectionString, "codebreaker");
});
builder.Services.AddSingleton<RandomGame6x4>();
builder.Services.AddSingleton<RandomGame8x5>();
builder.Services.AddSingleton<GameCache>();
builder.Services.AddTransient<Game6x4Service>();

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

app.MapPost("/v1/start", async (Game6x4Service service, CreateGameRequest request) =>
{
    using var activity = activitySource.StartActivity("Game started", ActivityKind.Server);
    gamesStarted.Add(1);

    string id = await service.StartGameAsync(request.Name, GameTypes.Game6x4);
    activity?.AddBaggage("GameId", id);
    activity?.AddBaggage("Name", request.Name);
    activity?.AddEvent(new ActivityEvent("Game started"));

    return Results.Ok(new CreateGameResponse(id, new CreateGameOptions(GameTypes.Game6x4, NumberFields: 4, MaxMoves: 12, "black", "white", "red", "green", "blue", "yellow")));
}).WithDisplayName("PostStart")
.Produces<CreateGameResponse>(StatusCodes.Status200OK);

app.MapPost("/v1/move", async (Game6x4Service service, MoveRequest request) =>
{
    try
    {
        using var activity = activitySource.StartActivity("Game Move", ActivityKind.Server);
        activity?.AddBaggage("GameId", request.Id);
        movesDone.Add(1);

        GameMove move = new(request.Id, request.MoveNumber, request.CodePegs.ToList());
        var result = await service.SetMoveAsync(move);
        MoveResponse response = new(result.GameId, result.Completed, result.Won, result.KeyPegs);
        return Results.Ok(response);
    }
    catch (GameException ex)
    {
        app.Logger.Error(ex, ex.Message);
        return Results.UnprocessableEntity(request);
    }
}).WithDisplayName("PostMove")
.Produces<MoveResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status422UnprocessableEntity);

app.MapGet("/v1/report", async (CodeBreakerContext context, DateTime? date) =>
{
    DateTime definedDate = date ?? DateTime.Today;

    app.Logger.GameReport(definedDate.ToString("yyyy-MM-dd"));

    definedDate = definedDate.Date;

    var games = await context.GetGamesAsync(definedDate);
    return Results.Ok(games);
}).WithDisplayName("GetReport")
.Produces<IEnumerable<GamesInfo>>(StatusCodes.Status200OK);

app.MapGet("/v1/reportdetail/{id}", async (CodeBreakerContext context, string id) =>
{
    app.Logger.DetailedGameReport(id);

    var games = await context.GetGameDetailAsync(id);
    return Results.Ok(games);
}).WithDisplayName("GetReportDetail")
.Produces<CodeBreakerGame>(StatusCodes.Status200OK);

app.Run();
