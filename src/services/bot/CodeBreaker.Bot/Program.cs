global using Microsoft.ApplicationInsights.Extensibility;
global using CodeBreaker.Bot;
global using System.Collections.Concurrent;

global using static CodeBreaker.Shared.Models.Data.Colors;

using System.Runtime.CompilerServices;
using CodeBreaker.APIs;
using CodeBreaker.Bot.Exceptions;
using CodeBreaker.Bot.Api;
using Microsoft.AspNetCore.Mvc;

[assembly: InternalsVisibleTo("MMBot.Tests")]

var builder = WebApplication.CreateBuilder(args);

// ApplicationInsights
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>();

WebApplication? app = null;

// Swagger & EndpointDocumentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient & Application Services
builder.Services.AddHttpClient<CodeBreakerGameRunner>(options =>
{
    // running with tye?
    Uri? apiUri = builder.Configuration.GetServiceUri("codebreaker-apis");
    
    if (apiUri is null)
    {
        string codebreakeruri = builder.Configuration["ApiBase"]
            ?? throw new InvalidOperationException("ApiBase configuration not available"); ;

        apiUri = new Uri(codebreakeruri);
    }

    app?.Logger.UsingUri(apiUri.ToString());
    options.BaseAddress = apiUri;
});
builder.Services.AddScoped<CodeBreakerTimer>();

app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/bots", (
    [FromServices] CodeBreakerTimer timer,
    [FromQuery] int? delay,
    [FromQuery] int? count,
    [FromQuery] int? thinkTime) =>
{
    Guid id;

    try
    {
        id = timer.Start(delay ?? 60, count ?? 3, thinkTime ?? 3);
    }
    catch (ArgumentOutOfRangeException)
    {
        return Results.BadRequest("Invalid options");
    }

    return Results.Accepted($"/games/{id}", id);
})
.Produces(StatusCodes.Status202Accepted)
.Produces(StatusCodes.Status400BadRequest)
.WithName("CreateBot")
.WithSummary("Starts a bot playing one or more games")
.WithOpenApi(x =>
{
    x.Parameters[0].Description = "The delay between the games (seconds). If not specified, default values are used.";
    x.Parameters[1].Description = "The number of games to play. If not specified, default values are used.";
    x.Parameters[2].Description = "The think time between game moves (seconds). If not specified, default values are used.";
    return x;
});

app.MapGet("/bots/{id}", (Guid id) =>
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
.Produces<StatusResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetBot")
.WithSummary("Gets the status of a bot")
.WithOpenApi(x =>
{
    x.Parameters[0].Description = "The id of the bot";
    return x;
});

app.MapDelete("/bots/{id}", (Guid id) =>
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
.Produces(StatusCodes.Status404NotFound)
.WithName("StopBot")
.WithSummary("Stops the bot with the given id")
.WithOpenApi(x =>
{
    x.Parameters[0].Description = "The id of the bot";
    return x;
});

app.Run();
