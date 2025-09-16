using Azure.Messaging.EventHubs.Consumer;

using System.Runtime.CompilerServices;

namespace Codebreaker.Live.Endpoints;

public class StreamingLiveHub(EventHubConsumerClient consumerClient, ILogger<StreamingLiveHub> logger) : Hub
{
    public async IAsyncEnumerable<GameSummary> SubscribeToGameCompletions(string gameType, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PartitionEvent ev in consumerClient.ReadEventsAsync(cancellationToken))
        {
            GameSummary? gameSummary;
            try
            {
                logger.ProcessingGameCompletionEvent();
                gameSummary = ev.Data.EventBody.ToObjectFromJson<GameSummary>();
                if (gameSummary is null)
                {
                    logger.GameSummaryIsNull();
                    continue;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorProcessingGameCompletionEvent(ex, ex.Message);
                continue;
            }

            if (gameSummary.GameType == gameType)
            {
                yield return gameSummary;
            }
            else
            {
                continue;
            }
        }
    }
}
