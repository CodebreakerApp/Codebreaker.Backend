using CodeBreaker.Queuing.ReportService.Transfer;

namespace CodeBreaker.Queuing.ReportService.Services;

public interface IGameQueuePublisherService
{
    Task EnqueueMessageAsync(Game game, CancellationToken cancellationToken = default);
}
