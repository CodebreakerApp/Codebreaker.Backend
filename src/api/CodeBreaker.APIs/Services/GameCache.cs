namespace CodeBreaker.APIs.Services;

internal class GameCache
{
    private readonly ConcurrentDictionary<string, Game> _games = new();
    private readonly ILogger _logger;

    public GameCache(ILogger<GameCache> logger)
    {
        _logger = logger;
    }

    public void SetGame(Game game)
    {
        _games.TryAdd(game.GameId, game);
        _logger.LogInformation("new game set, currently {gamecount} active", _games.Count());
    }

    /// <summary>
    /// Returns the Game object for the specified identifier
    /// </summary>
    /// <param name="id">game identifier</param>
    /// <exception cref="KeyNotFoundException" />
    /// <returns>a Game object</returns>
    /// <see cref="Game"/>
    public Game GetGame(string id)
    {
        return _games[id];
    }

    public void DeleteGame(string id)
    {
        _games.TryRemove(id, out _);
    }
}
