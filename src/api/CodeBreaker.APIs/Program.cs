global using CodeBreaker.Shared;
global using System.Collections.Concurrent;
global using Microsoft.EntityFrameworkCore;
global using CodeBreaker.APIs.Data;
global using CodeBreaker.Shared.APIModels;
global using CodeBreaker.APIs.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ICodeBreakerContext, CodeBreakerContext>(options =>
{
    string connectionString = builder.Configuration.GetSection("CodeBreakerAPI").GetConnectionString("CodeBreakerConnection");
    options.UseCosmos(connectionString, "CodeBreaker");
});
builder.Services.AddTransient<IGameInitializer, RandomGameGenerator>();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddTransient<GameService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/start", async (GameService service, CreateGameRequest request) =>
{
    string id = await service.StartGameAsync(request.Name);

    return Results.Ok(new CreateGameResponse(id));
});

app.MapPost("/move", async (GameService service, MoveRequest request) =>
{
    GameMove move = new(request.Id, request.MoveNumber, request.CodePegs.ToList());
    var result = await service.SetMoveAsync(move);
    MoveResponse response = new(result.GameId, result.Completed, result.Won, result.KeyPegs);
    return Results.Ok(response);
});

app.Run();
