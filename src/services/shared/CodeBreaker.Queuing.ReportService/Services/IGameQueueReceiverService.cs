using CodeBreaker.Queuing.Common.Models;
using CodeBreaker.Queuing.ReportService.Transfer;

namespace CodeBreaker.Queuing.ReportService.Services;

public interface IGameQueueReceiverService
{
    Task<QueueMessage<Game>> DequeueMessageAsync(CancellationToken cancellationToken = default);

    Task<QueueMessage<Game>> DequeueAndRemoveMessageAsync(CancellationToken cancellationToken = default);

    Task RemoveMessageAsync(QueueMessage<Game> message, CancellationToken cancellationToken = default);
}
