using Microsoft.AspNetCore.SignalR;
using CodeBreaker.LiveService.Shared;
using CodeBreaker.LiveService;

namespace LiveService;

public class LiveHub : Hub
{
    private readonly IEventSourceService _eventSourceService;

    public LiveHub(IEventSourceService eventSourceService) =>
        _eventSourceService = eventSourceService;

    public IAsyncEnumerable<LiveHubArgs> SubscribeToGameEvents(CancellationToken token) =>
        _eventSourceService.SubscribeAsync(token)
            .Where(x => x.Name is not null);
}
