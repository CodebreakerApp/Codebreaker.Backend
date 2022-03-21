

namespace MM.APIs.Services;

public class GameManager
{
    private readonly ConcurrentDictionary<string, Game> _games = new();
    private readonly ILogger _logger;

    public GameManager(ILogger<GameManager> logger)
    {
        _logger = logger;
    }

    public void SetGame(Game game)
    {
        _games.TryAdd(game.GameId, game);
        _logger.LogInformation("new game set, currently {gamecount} active", _games.Count());
    }

    public Game GetGame(string id)
    {
        return _games[id];
    }

    public void DeleteGame(string id)
    {
        _games.TryRemove(id, out _);
    }
}
