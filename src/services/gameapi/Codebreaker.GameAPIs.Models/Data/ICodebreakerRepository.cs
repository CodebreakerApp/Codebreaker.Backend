namespace Codebreaker.GameAPIs.Models.Data;
public interface ICodebreakerRepository
{
    Task CreateGameAsync(Game game);
    Task UpdateGameAsync(Game game);
    Task DeleteGameAsync(Guid gameId);
    Task<Game?> GetGameAsync(Guid gameId, bool withTracking = true);
    IAsyncEnumerable<Game> GetGamesByDateAsync(DateOnly date);
    Task<IEnumerable<Game>> GetMyGamesAsync(string gamerName);
}
