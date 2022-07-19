using System.Runtime.CompilerServices;
using Azure.Messaging.EventHubs.Consumer;
using CodeBreaker.LiveService.Shared;

namespace CodeBreaker.LiveService;

public class EventSourceService
{
    private readonly EventHubConsumerClient _eventHubConsumerClient;

    public EventSourceService(IConfiguration configuration)
    {
        const string consumerGroupConfigurationName = "Azure:EventHub:ConsumerGroupName";
        const string connectionStringConfigurationName = "Azure:EventHub:ConnectionString";
        const string nameConfigurationNae = "Azure:EventHub:Name";
        _eventHubConsumerClient = new EventHubConsumerClient(
            configuration[consumerGroupConfigurationName] ?? EventHubConsumerClient.DefaultConsumerGroupName,
            configuration[connectionStringConfigurationName] ?? throw new ArgumentNullException(connectionStringConfigurationName, $"{connectionStringConfigurationName} is not configured"),
            configuration[nameConfigurationNae] ?? throw new ArgumentNullException(nameConfigurationNae, $"{nameConfigurationNae} is not configured")
        );
    }

    public async IAsyncEnumerable<LiveHubArgs> SubscribeAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (PartitionEvent e in _eventHubConsumerClient.ReadEventsAsync(token))
            yield return e.Data.EventBody.ToObjectFromJson<LiveHubArgs>();
    }
}
