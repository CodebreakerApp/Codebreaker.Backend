using static CodeBreaker.Shared.CodeBreakerColors;

namespace CodeBreaker.APIs.Services;

internal class Game8x5Service : IGameService
{
    private int _holes;
    private readonly RandomGame8x5 _gameGenerator;
    private readonly GameCache _gameCache;
    private readonly ILogger _logger;
    private readonly ICodeBreakerContext _efContext;

    public Game8x5Service(
        RandomGame8x5 gameGenerator,
        GameCache gameCache,
        ICodeBreakerContext context,
        ILogger<Game8x5Service> logger)
    {
        _gameGenerator = gameGenerator;
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
            GameMoveResult result = new(guess.GameId, guess.MoveNumber);

            var game = _gameCache.GetGame(guess.GameId);
            if (game is null)
            {
                _logger.GameNotCached(guess.GameId);
                // game is not in the cache, so we need to get it from the data store
                game = await GetGameFromDatabaseAsync();
            }

            // TODO: check for mismatch of moves! if (guess.MoveNumber != game.codd)
            
            if (guess.MoveNumber > 12)
            {
                CodeBreakerGame dataGame = game.ToDataGame();
                await _efContext.UpdateGameAsync(dataGame);

                result = result with { Completed = true };
                return result;
            }

            List<string> codes = new(game.Code); // copy the codes from the game to check the codes that are already calculated
            List<string> guesses = new(guess.GuessPegs); // copy the moves to check the moves that are already calculated
            List<int> blackHits = new(); // correct positions where blacks are found
            List<string> keyPegs = new(); // the final information for the keys, black and white pegs are added

            // first check for the correct position
            for (int i = 0; i < _holes; i++)
            {
                if (codes[i] == guess.GuessPegs[i])
                {
                    keyPegs.Add(Black);
                    blackHits.Add(i);
                }
            }

            // remove the moves that may not be checked when checking corrects for wrong position
            for (int i = blackHits.Count - 1; i >= 0; i--)
            {
                codes.RemoveAt(blackHits[i]);
                guesses.RemoveAt(blackHits[i]);
            }

            // second check for corrects with the wrong position
            keyPegs = GetWhiteKeyPegs(codes, guesses, keyPegs);

            // sort the pegs, no hint about the position
            keyPegs.Sort();

            foreach (var keyPeg in keyPegs)
            {
                result.KeyPegs.Add(keyPeg);
            }

            CodeBreakerGameMove dataMove = new(
                Guid.NewGuid().ToString(),
                game.GameId,
                guess.MoveNumber,
                string.Join("..", guess.GuessPegs),
                DateTime.Now,
                string.Join(".", result.KeyPegs),
                string.Join("..", game.Code));
            
            // write the move to the data store
            await _efContext.AddMoveAsync(dataMove);

            // write the complete game to the data store if it finished
            if (result.KeyPegs.Count(s => s == Black) == 4)
            {
                result = result with { Won = true };
                CodeBreakerGame efGame = new(game.GameId, GameTypes.Game8x5, string.Join("..", game.Code), game.Name, DateTime.Now);
                await _efContext.UpdateGameAsync(efGame);
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

    private List<string> GetWhiteKeyPegs(List<string> codes, List<string> moves, List<string> keyPegs)
    {
        List<KeyPegWithFlag> tempCode = new(codes.Select(c => new KeyPegWithFlag(c, false)).ToArray());
        List<KeyPegWithFlag> tempMoves = new(moves.Select(m => new KeyPegWithFlag(m, false)).ToArray());

        for (int i = 0; i < tempCode.Count; i++)
        {
            int j = 0;
            bool pegAdded = false;
            while (j < tempMoves.Count && !pegAdded)
            {
                if (!tempCode[i].Used && !tempMoves[j].Used && tempCode[i].Value == tempMoves[j].Value)
                {
                    keyPegs.Add(White);
                    tempCode[i] = tempCode[i] with { Used = true };
                    tempMoves[j] = tempMoves[j] with { Used = true };
                    pegAdded = true;
                }
                j++;
            }
        }

        return keyPegs;
    }
}
