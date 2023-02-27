using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using CodeBreaker.Shared.Models.Live;
using CodeBreaker.Shared.Models.Live.EventPayloads;
using static CodeBreaker.Shared.Models.Live.LiveEventNames;

namespace CodeBreaker.APIs.Services;

public sealed class EventService : IPublishEventService, IAsyncDisposable
{
    private readonly EventHubProducerClient _eventHubPublisherClient;

    public EventService(EventHubProducerClient eventHubProducerClient)
    {
        _eventHubPublisherClient = eventHubProducerClient;
    }

    public Task FireGameCreatedEventAsync(GameCreatedPayload payload, CancellationToken token = default) =>
        SendEventAsync(GameCreatedEventName, payload, token);

    public Task FireMoveCreatedEventAsync(MoveCreatedPayload payload, CancellationToken token = default) =>
        SendEventAsync(MoveCreatedEventName, payload, token);

    private async Task SendEventAsync<TData>(string eventName, TData payload, CancellationToken token = default)
        where TData : notnull
    {
        LiveHubArgs args = new (eventName, payload);
        using EventDataBatch eventBatch = await _eventHubPublisherClient.CreateBatchAsync(token);

        if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(args)))))
            throw new EventBatchSizeException("Could not add game to the eventBatch. The payload is too large. (https://docs.microsoft.com/en-us/dotnet/api/microsoft.servicebus.messaging.eventdatabatch.tryadd?view=azure-dotnet#remarks)");

        await _eventHubPublisherClient.SendAsync(eventBatch, token);
    }

    ValueTask IAsyncDisposable.DisposeAsync() =>
        _eventHubPublisherClient.DisposeAsync();
}
