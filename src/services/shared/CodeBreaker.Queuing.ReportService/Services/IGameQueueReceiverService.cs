using CodeBreaker.Queuing.Common.Models;
using CodeBreaker.Queuing.ReportService.Transfer;

namespace CodeBreaker.Queuing.ReportService.Services;

public interface IGameQueueReceiverService
{
    Task<QueueMessage<GameDto>> DequeueMessageAsync(CancellationToken cancellationToken = default);

    Task<QueueMessage<GameDto>> DequeueAndRemoveMessageAsync(CancellationToken cancellationToken = default);

    Task RemoveMessageAsync(QueueMessage<GameDto> message, CancellationToken cancellationToken = default);
}
