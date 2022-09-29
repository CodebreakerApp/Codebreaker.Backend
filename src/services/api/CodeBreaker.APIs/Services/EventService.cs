using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using CodeBreaker.Shared.Exceptions;
using CodeBreaker.Shared.Models.Data;
using CodeBreaker.Shared.Models.Live;
using static CodeBreaker.Shared.Models.Live.LiveEventNames;

namespace CodeBreaker.APIs.Services;

public class EventService : IPublishEventService, IAsyncDisposable
{
    private readonly EventHubProducerClient _eventHubPublisherClient;

    public EventService(EventHubProducerClient eventHubProducerClient)
    {
        _eventHubPublisherClient = eventHubProducerClient;
    }

    public Task FireGameCreatedEventAsync(Game game, CancellationToken token = default) =>
        SendEventAsync(GameCreatedEventName, game, token);

    public Task FireMoveCreatedEventAsync(Move move, CancellationToken token = default) =>
        SendEventAsync(MoveCreatedEventName, move, token);

    private async Task SendEventAsync<TData>(string eventName, TData data, CancellationToken token = default)
        where TData : notnull
    {
        LiveHubArgs args = new (eventName, data);
        using EventDataBatch eventBatch = await _eventHubPublisherClient.CreateBatchAsync(token);

        if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(args)))))
            throw new EventBatchSizeException("Could not add game to the eventBatch. The payload is too large. (https://docs.microsoft.com/en-us/dotnet/api/microsoft.servicebus.messaging.eventdatabatch.tryadd?view=azure-dotnet#remarks)");

        await _eventHubPublisherClient.SendAsync(eventBatch, token);
    }

    ValueTask IAsyncDisposable.DisposeAsync() =>
        _eventHubPublisherClient.DisposeAsync();
}
