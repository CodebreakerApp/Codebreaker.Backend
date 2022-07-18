using Azure.Messaging.EventGrid;
using LiveService;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR().AddAzureSignalR();

WebApplication app = builder.Build();

app.MapHub<LiveHub>("/live");

app.MapPost("/event", (
    [FromBody] EventGridEvent[] events,
    [FromServices] LiveHub hub,
    CancellationToken token
) =>
{
    foreach (EventGridEvent e in events)
        hub.FireGameEventAsync(new(e.EventType, e.Data), token);
});

app.Run();
