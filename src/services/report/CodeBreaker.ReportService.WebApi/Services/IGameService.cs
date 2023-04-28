using CodeBreaker.Data.ReportService.Models;

namespace CodeBreaker.ReportService.Services;
public interface IGameService
{
    IQueryable<Game> Games { get; }
    ValueTask<Game> GetAsync(Guid id);
    IAsyncEnumerable<Game> GetByDate(DateTime from, DateTime to);
}
