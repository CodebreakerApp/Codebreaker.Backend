namespace CodeBreaker.APIs.Services;

internal class Game8x5Service : IGameService
{
    private int _holes;
    private readonly Game8x5Definition _gameDefinition;
    private readonly IGameAlgorithm _gameAlgorithm;
    private readonly IGameCache _gameCache;
    private readonly ILogger _logger;
    private readonly ICodeBreakerContext _efContext;

    public Game8x5Service(
        Game8x5Definition gameDefinition,
        IGameAlgorithm gameAlgorithm,
        IGameCache gameCache,
        ICodeBreakerContext context,
        ILogger<Game8x5Service> logger)
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
        _holes = _gameDefinition.Holes;
        
        Game game = new(Guid.NewGuid(), gameType, username, code, _gameDefinition.Colors, _gameDefinition.Holes, _gameDefinition.MaxMoves, DateTime.Now);
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
