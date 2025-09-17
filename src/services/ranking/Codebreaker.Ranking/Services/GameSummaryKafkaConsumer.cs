using Codebreaker.GameAPIs.Models;
using Codebreaker.Ranking.Data;
using Codebreaker.Ranking.Extensions;

using Confluent.Kafka;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;

namespace Codebreaker.Ranking.Services;

public class GameSummaryKafkaConsumer(IConsumer<string, string> kafkaClient, IDbContextFactory<RankingsContext> factory, ILogger<GameSummaryEventProcessor> logger) : IGameSummaryProcessor
{
    public async Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {

        kafkaClient.Subscribe("ranking");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = kafkaClient.Consume(cancellationToken);
                    var value = result.Message.Value;
                    var summary = JsonSerializer.Deserialize<GameSummary>(value);

                    if (summary is null)
                    {
                        logger.DeserializedNullGameSummary();
                        continue;
                    }

                    using var context = await factory.CreateDbContextAsync(cancellationToken);
                    await context.AddGameSummaryAsync(summary, cancellationToken);
                }
                catch (ConsumeException ex) when (ex.HResult == -2146233088)
                {
                    logger.ConsumeException(ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.ProcessingWasCancelled();
        }
        catch (Exception ex)
        {
            logger.Error(ex, ex.Message);
            throw;
        }
    }

    public Task StopProcessingAsync(CancellationToken cancellationToken = default)
    {
        kafkaClient.Unsubscribe();
        return Task.CompletedTask;
    }
}
