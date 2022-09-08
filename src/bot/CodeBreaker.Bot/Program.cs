global using Microsoft.ApplicationInsights.Extensibility;
global using CodeBreaker.Bot;
global using System.Collections.Concurrent;

global using static CodeBreaker.Shared.Models.Data.Colors;

using System.Runtime.CompilerServices;
using CodeBreaker.APIs;

[assembly: InternalsVisibleTo("MMBot.Tests")]

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
builder.Services.AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>();
WebApplication? app = null;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<CodeBreakerGameRunner>(options =>
{
    // running with tye?
    Uri? apiUri = builder.Configuration.GetServiceUri("codebreaker-apis");
    
    if (apiUri is null)
    {
        string? codebreakeruri = builder.Configuration.GetSection("CodeBreakerBot")["APIUri"];
        if (codebreakeruri is null) throw new InvalidOperationException("APIURI not retrieved");

        apiUri = new Uri(codebreakeruri);
    }
    app?.Logger.UsingUri(apiUri.ToString());
    options.BaseAddress = apiUri;
});
builder.Services.AddScoped<CodeBreakerTimer>();

app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/games", (CodeBreakerTimer timer, int? delaySecondsBetweenGames, int? numberGames, int? thinkSeconds) =>
{
    string id = timer.Start(delaySecondsBetweenGames ?? 60, numberGames ?? 3, thinkSeconds ?? 3);

    return Results.Accepted($"/games/{id}", id);
}).Produces(StatusCodes.Status202Accepted);

app.MapGet("/games/{id}", (string id) =>
{
    string status = CodeBreakerTimer.Status(id);
    return Results.Ok(status);
}).Produces(StatusCodes.Status200OK);

app.MapDelete("/games/{id}", (string id) =>
{
    string result = CodeBreakerTimer.Stop(id);
    return Results.Ok(result);
}).Produces(StatusCodes.Status200OK);

app.Run();
