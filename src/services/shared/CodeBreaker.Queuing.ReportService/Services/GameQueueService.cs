using Azure.Storage.Queues;
using CodeBreaker.Queuing.Common.Services;
using CodeBreaker.Queuing.ReportService.Options;
using CodeBreaker.Queuing.ReportService.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeBreaker.Queuing.ReportService.Services;

public class GameQueueService : QueueService<GameDto>, IGameQueueReceiverService, IGameQueuePublisherService
{
    public GameQueueService(ILogger<GameQueueService> logger, QueueServiceClient queueServiceClient, IOptions<GameQueueOptions> options) : base(logger, queueServiceClient, options.Value.Name)
    {
    }
}
