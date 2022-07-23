using CodeBreaker.LiveService.Shared;
using Microsoft.AspNetCore.SignalR.Client;

namespace CodeBreaker.Services;

public class LiveClient
{
    private readonly HubConnection _hubConnection;

    public LiveClient(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;
    }

    public Task StartAsync(CancellationToken token = default)
    {
        return _hubConnection.StartAsync(token);
    }

    public IAsyncEnumerable<LiveHubArgs> SubscribeToEventsAsync(CancellationToken token = default)
    {
        if (_hubConnection.State != HubConnectionState.Connected)
            throw new InvalidOperationException("The SignalR-Client has to be started before");

        return _hubConnection.StreamAsync<LiveHubArgs>("SubscribeToGameEvents", token);
    }
}
