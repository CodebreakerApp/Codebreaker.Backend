using System.Runtime.CompilerServices;
using Azure.Messaging.EventHubs.Consumer;
using CodeBreaker.LiveService.Shared;

namespace CodeBreaker.LiveService;

public class EventSourceService
{
    private readonly EventHubConsumerClient _eventHubConsumerClient;

    public EventSourceService(EventHubConsumerClient eventHubConsumerClient)
    {
        _eventHubConsumerClient = eventHubConsumerClient;
    }

    public async IAsyncEnumerable<LiveHubArgs> SubscribeAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (PartitionEvent e in _eventHubConsumerClient.ReadEventsAsync(token))
            yield return e.Data.EventBody.ToObjectFromJson<LiveHubArgs>();
    }
}
