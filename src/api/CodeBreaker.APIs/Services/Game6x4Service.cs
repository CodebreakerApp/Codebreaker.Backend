using static CodeBreaker.Shared.CodeBreakerColors;

namespace CodeBreaker.APIs.Services;

public record KeyPegWithFlag(string Value, bool Used);

internal class Game6x4Service : IGameService
{
    private readonly Game6x4Definition _gameDefinition;
    private readonly GameAlgorithm _gameAlgorithm;
    private readonly GameCache _gameCache;
    private readonly ICodeBreakerContext _efContext;
    private readonly ILogger _logger;


    public Game6x4Service(
        Game6x4Definition gameDefinition,
        GameAlgorithm gameAlgorithm,
        GameCache gameCache,
        ICodeBreakerContext context,
        ILogger<Game6x4Service> logger)
    {
        _gameDefinition = gameDefinition;
        _gameAlgorithm = gameAlgorithm;
        _gameCache = gameCache;
        _logger = logger;
        _efContext = context;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <param name="gameType"></param>
    /// <returns></returns>
    public async Task<Game> StartGameAsync(string username, string gameType)
    {
        string[] code = _gameDefinition.CreateRandomCode();

        Game game = new(Guid.NewGuid().ToString(), gameType, username, code, _gameDefinition.Colors, _gameDefinition.Holes, DateTime.Now);
        _gameCache.SetGame(game);

        await _efContext.InitGameAsync(game.ToDataGame());

        _logger.GameStarted(game.ToString());

        return game;
    }

    public async Task<GameMoveResult> SetMoveAsync(GameMove guess)
    {       
        async Task<Game> GetGameFromDatabaseAsync()
        {
            var dbGame = await _efContext.GetGameAsync(guess.GameId);
            if (dbGame is null)
            {
                _logger.GameIdNotFound(guess.GameId);
                throw new GameException("Game id not found");
            }
            return dbGame.ToGame();
        }
        
        try
        {
            // get server-side game information from the in-process cache or the database
            var game = _gameCache.GetGame(guess.GameId);
            if (game is null)
            {
                _logger.GameNotCached(guess.GameId);
                // game is not in the cache, so we need to get it from the data store
                game = await GetGameFromDatabaseAsync();
            }

            (GameMoveResult result, CodeBreakerGame dataGame, CodeBreakerGameMove? dataMove) = _gameAlgorithm.SetMove(game, guess);

            // write the move to the data store
            if (dataMove is not null)
            {
                await _efContext.AddMoveAsync(dataMove);
            }

            if (result.Completed)
            {
                await _efContext.UpdateGameAsync(dataGame);
                return result;
            }

            _logger.SetMove(guess.ToString(), result.ToString());
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
            throw;
        }
    }
}
