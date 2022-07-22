using CodeBreaker.Shared;

namespace CodeBreaker.Services;
public interface IGameReportClient
{
    Task<CodeBreakerGame?> GetDetailedReportAsync(Guid id);
    Task<IEnumerable<GamesInfo>?> GetReportAsync(DateTime? date);
}
