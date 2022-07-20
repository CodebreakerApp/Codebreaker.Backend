using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using CodeBreaker.APIs.Options;
using CodeBreaker.LiveService.Shared;
using Microsoft.Extensions.Options;
using static CodeBreaker.LiveService.Shared.LiveEventNames;

namespace CodeBreaker.APIs.Services;

public class EventService : IPublishEventService
{
    private readonly EventHubProducerClient _eventGridPublisherClient;

    public EventService(IOptions<AzureOptions> azureOptions)
    {
        _eventGridPublisherClient = new (azureOptions.Value.EventHub.ConnectionString, azureOptions.Value.EventHub.Name);
    }

    public Task FireGameCreatedEventAsync(Game game, CancellationToken token = default) =>
        SendEventAsync(GameCreatedEventName, game, token);

    public Task FireMoveCreatedEventAsync(GameMove move, CancellationToken token = default) =>
        SendEventAsync(MoveCreatedEventName, move, token);

    private async Task SendEventAsync<TData>(string eventName, TData data, CancellationToken token = default)
        where TData : notnull
    {
        LiveHubArgs args = new (eventName, data);
        using EventDataBatch eventBatch = await _eventGridPublisherClient.CreateBatchAsync(token);

        if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(args)))))
            throw new EventBatchSizeException("Could not add game to the eventBatch. The payload is too large. (https://docs.microsoft.com/en-us/dotnet/api/microsoft.servicebus.messaging.eventdatabatch.tryadd?view=azure-dotnet#remarks)");

        await _eventGridPublisherClient.SendAsync(eventBatch, token);
    }
}
