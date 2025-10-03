using Azure.Messaging.EventHubs;

using Codebreaker.GameAPIs.Models;
using Codebreaker.Ranking.Data;
using Codebreaker.Ranking.Extensions;

using Microsoft.EntityFrameworkCore;

namespace Codebreaker.Ranking.Services;

public class GameSummaryEventProcessor(EventProcessorClient client, IDbContextFactory<RankingsContext> factory, ILogger<GameSummaryEventProcessor> logger) : IGameSummaryProcessor
{
    public async Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
        client.ProcessEventAsync += async (args) =>
        {
            logger.ProcessingEvent();

            GameSummary? summary = args.Data.EventBody.ToObjectFromJson<GameSummary>();

            if (summary is null)
            {
                logger.GameSummaryIsEmpty();
                return;
            }

            logger.ReceivedGameCompletionEvent(summary.Id);
            using var context = await factory.CreateDbContextAsync(cancellationToken);

            await context.AddGameSummaryAsync(summary, cancellationToken);
            await args.UpdateCheckpointAsync(cancellationToken);
        };

        client.ProcessErrorAsync += (args) =>
        {
            logger.ErrorProcessingEvent(args.Exception, args.Exception.Message);
            return Task.CompletedTask;
        };

        await client.StartProcessingAsync(cancellationToken);
    }

    public Task StopProcessingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return client.StopProcessingAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Error(ex, ex.Message);
            throw;
        }
    }
}
