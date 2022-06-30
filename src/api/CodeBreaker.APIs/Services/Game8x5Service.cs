﻿namespace CodeBreaker.APIs.Services;

internal class Game8x5Service : IGameService
{
    private int _holes;
    private readonly RandomGame8x5 _gameGenerator;
    private readonly GameAlgorithm _gameAlgorithm;
    private readonly GameCache _gameCache;
    private readonly ILogger _logger;
    private readonly ICodeBreakerContext _efContext;

    public Game8x5Service(
        RandomGame8x5 gameGenerator,
        GameAlgorithm gameAlgorithm,
        GameCache gameCache,
        ICodeBreakerContext context,
        ILogger<Game8x5Service> logger)
    {
        _gameGenerator = gameGenerator;
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
    public async Task<string> StartGameAsync(string username, string gameType)
    {
        string[] code = _gameGenerator.GetCode();
        _holes = _gameGenerator.Holes;
        
        Game game = new(Guid.NewGuid().ToString(), gameType, username, code);
        _gameCache.SetGame(game);

        await _efContext.InitGameAsync(game.ToDataGame());

        _logger.GameStarted(game.ToString());

        return game.GameId;
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

            (GameMoveResult result, CodeBreakerGame dataGame, CodeBreakerGameMove? dataMove) = _gameAlgorithm.SetMove(game, guess, _holes);

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
