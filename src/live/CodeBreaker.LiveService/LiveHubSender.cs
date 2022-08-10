using CodeBreaker.LiveService.Shared;
using LiveService;
using Microsoft.AspNetCore.SignalR;

namespace CodeBreaker.LiveService;

public class LiveHubSender : ILiveHubSender
{
    private readonly IHubContext<LiveHub> _hubContext;

    public LiveHubSender(IHubContext<LiveHub> hubContext) =>
        _hubContext = hubContext;

    public Task FireGameEvent(LiveHubArgs args, CancellationToken token) =>
        _hubContext.Clients.All.SendAsync("gameEvent", args, token);
}
