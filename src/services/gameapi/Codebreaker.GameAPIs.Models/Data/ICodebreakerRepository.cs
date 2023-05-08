namespace Codebreaker.GameAPIs.Models.Data;
public interface ICodebreakerRepository
{
    Task CreateGameAsync(Game game, CancellationToken cancellationToken = default);
    Task UpdateGameAsync(Game game, CancellationToken cancellationToken = default);
    Task DeleteGameAsync(Guid gameId, CancellationToken cancellationToken = default);
    Task<Game?> GetGameAsync(Guid gameId, bool withTracking = true, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Game> GetGamesByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Game>> GetMyGamesAsync(string gamerName, CancellationToken cancellationToken = default);
}
