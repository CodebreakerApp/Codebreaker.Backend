using System.Runtime.CompilerServices;
using Azure.Messaging.EventHubs.Consumer;
using CodeBreaker.LiveService.Options;
using CodeBreaker.LiveService.Shared;
using Microsoft.Extensions.Options;

namespace CodeBreaker.LiveService;

public class EventSourceService
{
    private readonly EventHubConsumerClient _eventHubConsumerClient;

    public EventSourceService(IOptions<AzureOptions> azureOptions)
    {
        const string consumerGroupConfigurationName = "Azure:EventHub:ConsumerGroupName";
        const string connectionStringConfigurationName = "Azure:EventHub:ConnectionString";
        const string nameConfigurationNae = "Azure:EventHub:Name";
        _eventHubConsumerClient = new EventHubConsumerClient(
            azureOptions.Value.EventHub.ConsumerGroupName ?? EventHubConsumerClient.DefaultConsumerGroupName,
            azureOptions.Value.EventHub.ConnectionString ?? throw new ArgumentNullException(connectionStringConfigurationName, $"{connectionStringConfigurationName} is not configured"),
            azureOptions.Value.EventHub.Name ?? throw new ArgumentNullException(nameConfigurationNae, $"{nameConfigurationNae} is not configured")
        );
    }

    public async IAsyncEnumerable<LiveHubArgs> SubscribeAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (PartitionEvent e in _eventHubConsumerClient.ReadEventsAsync(token))
            yield return e.Data.EventBody.ToObjectFromJson<LiveHubArgs>();
    }
}
