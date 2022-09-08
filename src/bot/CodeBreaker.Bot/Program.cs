global using Microsoft.ApplicationInsights.Extensibility;
global using CodeBreaker.Bot;
global using System.Collections.Concurrent;

global using static CodeBreaker.Shared.Models.Data.Colors;

using System.Runtime.CompilerServices;
using CodeBreaker.APIs;
using CodeBreaker.Bot.Exceptions;
using CodeBreaker.Bot.Api;

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
    Guid id;

    try
    {
        id = timer.Start(delaySecondsBetweenGames ?? 60, numberGames ?? 3, thinkSeconds ?? 3);
    }
    catch (ArgumentOutOfRangeException)
    {
        return Results.BadRequest("Invalid parameters");
    }

    return Results.Accepted($"/games/{id}", id);
})
.Produces(StatusCodes.Status202Accepted)
.Produces(StatusCodes.Status400BadRequest);

app.MapGet("/games/{id}", (Guid id) =>
{
    StatusResponse result;

    try
    {
        result = CodeBreakerTimer.Status(id);
    }
    catch (ArgumentException)
    {
        return Results.BadRequest("Invalid id");
    }
    catch (BotNotFoundException)
    {
        return Results.NotFound();
    }
    
    return Results.Ok(result);
})
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

app.MapDelete("/games/{id}", (Guid id) =>
{
    try
    {
        CodeBreakerTimer.Stop(id);
    }
    catch (ArgumentException)
    {
        return Results.BadRequest("Invalid id");
    }
    catch (BotNotFoundException)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

app.Run();
