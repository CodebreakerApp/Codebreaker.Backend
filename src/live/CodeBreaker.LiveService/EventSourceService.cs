using System.Runtime.CompilerServices;
using Azure.Messaging.EventHubs.Consumer;
using CodeBreaker.LiveService.Shared;

namespace CodeBreaker.LiveService;

public class EventSourceService : IEventSourceService
{
    private readonly ILogger _logger;

    private readonly EventHubConsumerClient _eventHubConsumerClient;

    public EventSourceService(ILogger<EventSourceService> logger, EventHubConsumerClient eventHubConsumerClient)
    {
        _logger = logger;
        _eventHubConsumerClient = eventHubConsumerClient;
    }

    public async IAsyncEnumerable<LiveHubArgs> SubscribeAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (PartitionEvent partitionEvent in _eventHubConsumerClient.ReadEventsAsync(token))
        {
            LiveHubArgs args;

            try
            {
                args = partitionEvent.Data.EventBody.ToObjectFromJson<LiveHubArgs>();
            }
            catch (ArgumentNullException e)
            {
                _logger.LogWarning(e, "Null-values assigned to LiveHubArgs-Properties. Skipping...");
                continue;
            }

            _logger.LogDebug($"Sending LiveHubArgs - {args.Name}", args);

            yield return args;
        }
    }
}
