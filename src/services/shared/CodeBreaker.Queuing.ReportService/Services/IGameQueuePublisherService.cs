using CodeBreaker.Queuing.ReportService.Transfer;

namespace CodeBreaker.Queuing.ReportService.Services;

public interface IGameQueuePublisherService
{
    Task EnqueueMessageAsync(GameDto game, CancellationToken cancellationToken = default);
}
