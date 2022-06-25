namespace CodeBreaker.APIs.Services;

public record KeyPegWithFlag(string Value, bool Used);

internal class Game6x4Service
{
    private const string black = nameof(black);
    private const string white = nameof(white);

    private const int Holes = 4;

    private readonly RandomGame6x4Generator _gameGenerator;
    private readonly GameCache _gameCache;
    private readonly ILogger _logger;
    private readonly ICodeBreakerContext _efContext;

    public Game6x4Service(
        RandomGame6x4Generator gameGenerator,
        GameCache gameCache,
        ICodeBreakerContext context,
        ILogger<Game6x4Service> logger)
    {
        _gameGenerator = gameGenerator;
        _gameCache = gameCache;
        _logger = logger;
        _efContext = context;
    }

    public async Task<string> StartGameAsync(string username, string gameType)
    {
        string[] code = _gameGenerator.GetPegs();
        Game game = new(Guid.NewGuid().ToString(), gameType, username, code);
        _gameCache.SetGame(game);

        await _efContext.InitGameAsync(game.ToDataGame());

        _logger.GameStarted(game.ToString());

        return game.GameId;
    }

    public async Task<GameMoveResult> SetMoveAsync(GameMove move)
    {
        try
        {
            GameMoveResult result = new(move.GameId, move.MoveNumber);

            var game = _gameCache.GetGame(move.GameId);
            if (game is null)
            {
                _logger.GameNotCached(move.GameId);

                // game is not in the cache, so we need to get it from the data store
                var dbGame = await _efContext.GetGameAsync(move.GameId);
                if (dbGame is null)
                {
                    _logger.GameIdNotFound(move.GameId);
                    throw new GameException("Game id not found");
                }
                game = dbGame.ToGame();
            }

            if (move.MoveNumber > 12)
            {
                CodeBreakerGame dataGame = game.ToDataGame();
                await _efContext.UpdateGameAsync(dataGame);

                result = result with { Completed = true };
                return result;
            }

            List<string> codes = new(game.Code); // copy the codes from the game to check the codes that are already calculated
            List<string> moves = new(move.CodePegs); // copy the moves to check the moves that are already calculated
            List<int> blackHits = new(); // correct positions where blacks are found
            List<string> keyPegs = new(); // the final information for the keys, black and white pegs are added

            // first check for the correct position
            for (int i = 0; i < Holes; i++)
            {
                if (codes[i] == move.CodePegs[i])
                {
                    keyPegs.Add(black);
                    blackHits.Add(i);
                }
            }

            // remove the moves that may not be checked when checking corrects for wrong position
            for (int i = blackHits.Count - 1; i >= 0; i--)
            {
                codes.RemoveAt(blackHits[i]);
                moves.RemoveAt(blackHits[i]);
            }

            // second check for corrects with the wrong position
            keyPegs = GetWhiteKeyPegs(codes, moves, keyPegs);

            // sort the pegs, no hint about the position
            keyPegs.Sort();

            foreach (var keyPeg in keyPegs)
            {
                result.KeyPegs.Add(keyPeg);
            }

            CodeBreakerGameMove dataMove = new(
                Guid.NewGuid().ToString(),
                game.GameId,
                move.MoveNumber,
                string.Join("..", move.CodePegs),
                DateTime.Now,
                string.Join(".", result.KeyPegs),
                string.Join("..", game.Code));
            
            // write the move to the data store
            await _efContext.AddMoveAsync(dataMove);

            // write the complete game to the data store if it finished
            if (result.KeyPegs.Count(s => s == black) == 4)
            {
                result = result with { Won = true };
                CodeBreakerGame efGame = new(game.GameId, GameTypes.Game6x4, string.Join("..", game.Code), game.Name, DateTime.Now);
                await _efContext.UpdateGameAsync(efGame);
            }

            _logger.SetMove(move.ToString(), result.ToString());
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
                    keyPegs.Add(white);
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
