namespace CodeBreaker.APIs.Services;

internal class GameCache : IGameCache
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();
    private readonly ILogger _logger;

    public GameCache(ILogger<GameCache> logger)
    {
        _logger = logger;
    }

    public void SetGame(Game game)
    {
        _games.TryAdd(game.GameId, game);
        _logger.GameCached(_games.Count);
    }

    /// <summary>
    /// Returns the Game object for the specified identifier
    /// </summary>
    /// <param name="id">game identifier</param>
    /// <exception cref="KeyNotFoundException" />
    /// <returns>a Game object</returns>
    /// <see cref="Game"/>
    public Game? GetGame(Guid id)
    {
        if (_games.TryGetValue(id, out var game))
        {
            return game;
        }
        else
        {
            return default;
        }
    }

    public void DeleteGame(Guid id)
    {
        _games.TryRemove(id, out _);
    }
}
