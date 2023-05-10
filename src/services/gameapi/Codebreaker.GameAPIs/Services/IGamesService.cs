namespace Codebreaker.GameAPIs.Services;

public interface IGamesService
{
    ValueTask<Game?> GetGameAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Game>> GetGamesRankByDateAsync(GameType gameType, DateOnly date, CancellationToken cancellationToken = default);

    // get the games from the last 24 hours which are not finished
    Task<IEnumerable<Game>> GetMyRunningGamesAsync(string playerName, CancellationToken cancellationToken = default);

    Task<IEnumerable<Game>> GetMyGamesAsync(string playerName, CancellationToken cancellationToken = default);

    Task<Game> StartGameAsync(GameType gameType, string playerName, CancellationToken cancellationToken = default);

    Task DeleteGameAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Game> SetMoveAsync(Guid gameId, Move move, CancellationToken cancellationToken = default);

    // user gives up - end and return the game with the result
    Task<Game> GiveUpAsync(Guid gameId);
}
