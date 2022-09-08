global using Microsoft.ApplicationInsights.Extensibility;
global using CodeBreaker.Bot;
global using System.Collections.Concurrent;

global using static CodeBreaker.Shared.Models.Data.Colors;

using System.Runtime.CompilerServices;
using CodeBreaker.APIs;
using CodeBreaker.Bot.Exceptions;
using CodeBreaker.Bot.Api;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Mvc;

[assembly: InternalsVisibleTo("MMBot.Tests")]

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
builder.Services.AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>();
WebApplication? app = null;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.EnableAnnotations());
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

app.MapPost("/bots", (
    [FromServices] CodeBreakerTimer timer,
    [FromQuery] [SwaggerParameter("The delay between the games (seconds). If not specified, default values are used.")] int? delay,
    [FromQuery] [SwaggerParameter("The number of games to play. If not specified, default values are used.")] int? count,
    [FromQuery] [SwaggerParameter("The duration of a game (seconds). If not specified, default values are used.")] int? duration) =>
{
    Guid id;

    try
    {
        id = timer.Start(delay ?? 60, count ?? 3, duration ?? 3);
    }
    catch (ArgumentOutOfRangeException)
    {
        return Results.BadRequest("Invalid options");
    }

    return Results.Accepted($"/games/{id}", id);
})
.Produces(StatusCodes.Status202Accepted)
.Produces(StatusCodes.Status400BadRequest)
.WithMetadata(new SwaggerOperationAttribute(summary: "Starts a bot playing one or more games"));

app.MapGet("/bots/{id}", ([SwaggerParameter("The id of the bot")] Guid id) =>
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
.WithMetadata(new SwaggerOperationAttribute(summary: "Gets the status of a bot"));

app.MapDelete("/bots/{id}", ([SwaggerParameter("The id of the bot")] Guid id) =>
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
.WithMetadata(new SwaggerOperationAttribute(summary: "Stops the bot with the given id"));

app.Run();
