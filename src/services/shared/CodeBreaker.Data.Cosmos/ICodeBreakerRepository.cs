using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.Data;
public interface ICodeBreakerRepository
{
    Task CreateGameAsync(Game game);
    Task DeleteGameAsync(Guid gameId);
    Task CancelGameAsync(Guid gameId);
    Task<Game?> GetGameAsync(Guid gameId, bool withTracking = true);
    IAsyncEnumerable<Game> GetGamesByDateAsync(DateOnly date);
    IAsyncEnumerable<Game> GetGamesByDateAsync(DateTime date);
    Task UpdateGameAsync(Game game);
}
