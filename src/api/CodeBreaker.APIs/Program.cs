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

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CodeBreaker.APIs.Tests")]

ActivitySource activitySource = new ActivitySource("CNinnovation.CodeBreaker.API");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
builder.Services.AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ICodeBreakerContext, CodeBreakerContext>(options =>
{
    string connectionString = builder.Configuration.GetSection("CodeBreakerAPI").GetConnectionString("CodeBreakerConnection");
    options.UseCosmos(connectionString, "codebreaker");
});
builder.Services.AddTransient<IGameInitializer, RandomGameGenerator>();
builder.Services.AddSingleton<GameCache>();
builder.Services.AddTransient<GameService>();

const string AllowCodeBreakerOrigins = "_allowCodeBreakerOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowCodeBreakerOrigins,
        builder =>
        {
            builder.AllowAnyOrigin() //.WithOrigins("https://localhost:7229", "http://localhost:5229")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors(AllowCodeBreakerOrigins);

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/v1/start", async (GameService service, CreateGameRequest request) =>
{
    using var activity = activitySource.StartActivity("Game started", ActivityKind.Server);

    string id = await service.StartGameAsync(request.Name);
    activity?.AddBaggage("GameId", id);
    activity?.AddBaggage("Name", request.Name);
    activity?.AddEvent(new ActivityEvent("Game started"));

    return Results.Ok(new CreateGameResponse(id));
}).WithDisplayName("PostStart")
.Produces<CreateGameResponse>(StatusCodes.Status200OK);

app.MapPost("/v1/move", async (GameService service, MoveRequest request) =>
{
    try
    {
        using var activity = activitySource.StartActivity("Game Move", ActivityKind.Server);
        activity?.AddBaggage("GameId", request.Id);

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

app.MapGet("/v1/reportdetail", async (CodeBreakerContext context, DateTime? date) =>
{
    DateTime definedDate = date ?? DateTime.Today;

    app.Logger.GameReport(definedDate.ToString("yyyy-MM-dd"));


    definedDate = definedDate.Date;

    var games = await context.GetGamesDetailsAsync(definedDate);
    return Results.Ok(games);
}).WithDisplayName("GetReportDetail")
.Produces<GamesInformationDetail>(StatusCodes.Status200OK);

app.Run();
