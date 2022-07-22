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

    public IAsyncEnumerable<LiveHubArgs> SubscribeToEventsAsync()
    {
        try
        {
            return _hubConnection.StreamAsync<LiveHubArgs>("SubscribeToGameEvents");
        }
        catch (Exception e)
        {

            throw;
        }
    }
}
