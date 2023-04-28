using CodeBreaker.Data.ReportService.Models;
using CodeBreaker.Data.ReportService.Repositories;

namespace CodeBreaker.ReportService.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;

    private readonly IQueryableGameRepository _queryableGameRepository;

    public GameService(IGameRepository gameRepository, IQueryableGameRepository queryableGameRepository)
    {
        _gameRepository = gameRepository;
        _queryableGameRepository = queryableGameRepository;
    }

    public IQueryable<Game> Games => _queryableGameRepository.QueryableGames;

    public IAsyncEnumerable<Game> GetByDate(DateTime from, DateTime to) =>
        _gameRepository.GetAsync(from, to);

    public async ValueTask<Game> GetAsync(Guid id) =>
        await _gameRepository.GetAsync(id);
}
