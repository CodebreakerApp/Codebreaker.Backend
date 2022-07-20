using Microsoft.AspNetCore.SignalR;
using CodeBreaker.LiveService.Shared;
using CodeBreaker.LiveService;

namespace LiveService;

public class LiveHub : Hub
{
    private readonly EventSourceService _eventSourceService;

    public LiveHub(EventSourceService eventSourceService) =>
        _eventSourceService = eventSourceService;

    public Task FireGameEvent(LiveHubArgs args, CancellationToken token) =>
        Clients.All.SendAsync("gameEvent", args, token);

    public IAsyncEnumerable<LiveHubArgs> SubscribeToGameEvents(CancellationToken token) =>
        _eventSourceService.SubscribeAsync(token);
}
