using Microsoft.AspNetCore.SignalR;
using CodeBreaker.LiveService.Shared;

namespace LiveService;

public class LiveHub : Hub
{
    public Task FireGameEventAsync(LiveHubArgs args, CancellationToken token) =>
        Clients.All.SendAsync("gameEvent", args, token);
}
