global using CodeBreaker.Shared;
global using System.Collections.Concurrent;
global using Microsoft.EntityFrameworkCore;
global using CodeBreaker.APIs;
global using CodeBreaker.APIs.Data;
global using CodeBreaker.Shared.APIModels;
global using CodeBreaker.APIs.Services;
global using CodeBreaker.APIs.Extensions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CodeBreaker.APIs.Tests")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
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
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

app.UseCors(AllowCodeBreakerOrigins);

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/v1/start", async (GameService service, CreateGameRequest request) =>
{
    string id = await service.StartGameAsync(request.Name);

    return Results.Ok(new CreateGameResponse(id));
}).WithDisplayName("PostStart")
.Produces<CreateGameResponse>(StatusCodes.Status200OK);

app.MapPost("/v1/move", async (GameService service, MoveRequest request) =>
{
    try
    {
        GameMove move = new(request.Id, request.MoveNumber, request.CodePegs.ToList());
        var result = await service.SetMoveAsync(move);
        MoveResponse response = new(result.GameId, result.Completed, result.Won, result.KeyPegs);
        return Results.Ok(response);
    }
    catch (GameException)
    {
        return Results.UnprocessableEntity(request);
    }
}).WithDisplayName("PostMove")
.Produces<MoveResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status422UnprocessableEntity);

app.MapGet("/v1/report", async (CodeBreakerContext context, DateTime? date, bool? detail) =>
{
    bool definedDetail = detail ?? false;
    DateTime definedDate = date ?? DateTime.Today;

    app.Logger.LogInformation("Requesting games for {date}", definedDate);

    definedDate = definedDate.Date;
    if (definedDetail)
    {
        var games = await context.GetGamesDetailsAsync(definedDate);
        return Results.Ok(games);
    }
    else
    {
        var games = await context.GetGamesAsync(definedDate);
        return Results.Ok(games);
    }
}).WithDisplayName("GetReport")
.Produces<IEnumerable<GamesInfo>>(StatusCodes.Status200OK)
.Produces<GamesInformationDetail>(StatusCodes.Status200OK);

app.Run();
