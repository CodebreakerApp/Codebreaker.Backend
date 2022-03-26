
using CodeBreaker.Bot;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MMBot.Tests")]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<CodeBreakerGameRunner>(options =>
{
    options.BaseAddress = new Uri("https://localhost:7053/api/MasterMind/");
});
builder.Services.AddScoped<CodeBreakerTimer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/start", (CodeBreakerTimer timer, int? delayseconds, int? loops) =>
{
    string id = timer.Start(delayseconds ?? 60, loops ?? 3);

    return Results.Accepted($"/status/{id}", id);
});

app.MapGet("/status/{id}", (string id) =>
{
    string status = CodeBreakerTimer.Status(id);
    return Results.Ok(status);
});

app.MapGet("/stop/{id}", (string id) =>
{
    string result = CodeBreakerTimer.Stop(id);
    return Results.Ok(result);
});


app.Run();
