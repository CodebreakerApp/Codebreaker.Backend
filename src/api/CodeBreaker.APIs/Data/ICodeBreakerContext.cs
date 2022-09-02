using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Data;
public interface ICodeBreakerContext
{
    DbSet<Game> Games { get; }

    Task<Game> AddMoveAsync(Guid gameId, Move move);
    Task<Game> AddMoveAsync(Game game, Move move);
    Task CreateGameAsync(Game game);
    Task DeleteGameAsync(Guid gameId);
    Task CancelGameAsync(Guid gameId);
    Task<Game?> GetGameAsync(Guid gameId);
    Task<IAsyncEnumerable<Game>> GetGamesByDateAsync(DateOnly date);
    Task<IAsyncEnumerable<Game>> GetGamesByDateAsync(DateTime date);
    Task UpdateGameAsync(Game game);
}
