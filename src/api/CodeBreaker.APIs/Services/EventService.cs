using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using CodeBreaker.LiveService.Shared;
using static CodeBreaker.LiveService.Shared.LiveEventNames;

namespace CodeBreaker.APIs.Services;

public class EventService : IEventService
{
    private readonly EventHubProducerClient _eventGridPublisherClient;

    public EventService(IConfiguration configuration)
    {
        _eventGridPublisherClient = new EventHubProducerClient(configuration["Azure:EventHub:ConnectionString"], configuration["Azure:EventHub:Name"]);
    }

    public Task FireGameCreatedEventAsync(Game game, CancellationToken token = default) =>
        SendEventAsync(GameCreatedEventName, game, token);

    public Task FireMoveCreatedEventAsync(GameMove move, CancellationToken token = default) =>
        SendEventAsync(MoveCreatedEventName, move, token);

    private async Task SendEventAsync<TData>(string eventName, TData data, CancellationToken token = default)
        where TData : notnull
    {
        LiveHubArgs args = new LiveHubArgs(eventName, data);
        using EventDataBatch eventBatch = await _eventGridPublisherClient.CreateBatchAsync(token);

        if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(args)))))
            throw new Exception("Could not add game to the eventBatch");

        await _eventGridPublisherClient.SendAsync(eventBatch);
    }
}
