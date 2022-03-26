namespace CodeBreaker.APIs.Services;

public record KeyPegWithFlag(string Value, bool Used);

public class GameService
{
    private const string black = nameof(black);
    private const string white = nameof(white);

    private const int Holes = 4;

    private readonly IGameInitializer _gameInitializer;
    private readonly GameManager _gameManager;
    private readonly ILogger _logger;
    private readonly IMastermindContext _efContext;
    public GameService(
        IGameInitializer gameInitializer, 
        GameManager gameManager,
        IMastermindContext context,
        ILogger<GameService> logger)
    {
        _gameInitializer = gameInitializer;
        _gameManager = gameManager;
        _logger = logger;
        _efContext = context;
    }

    public Task<string> StartGameAsync(string name)
    {
        string[] code = _gameInitializer.GetColors(Holes);
        Game game = new(Guid.NewGuid().ToString(), name, code);
        _gameManager.SetGame(game);
        _logger.LogInformation("Started a game with this information {game}", game);
        return Task.FromResult(game.GameId);
    }

    public async Task<GameMoveResult> SetMoveAsync(GameMove move)
    {
        GameMoveResult result = new(move.GameId, move.MoveNumber);

        var game = _gameManager.GetGame(move.GameId);

        if (move.MoveNumber > 12)
        {
            MasterMindGame efGame = new(game.GameId, string.Join("..", game.Code), game.Name, DateTime.Now);
            await _efContext.AddGameAsync(efGame); 
            
            result = result with { Completed = true };
            return result;
        }

        List<string> codes = new(game.Code); // temporary corrects
        List<string> moves = new(move.CodePegs);
        List<int> blackHits = new();
        List<string> keyPegs = new(); // the final information for the keys

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

        MasterMindGameMove dataMove = new(
            Guid.NewGuid().ToString(), 
            game.GameId, 
            move.MoveNumber, 
            string.Join("..", move.CodePegs),
            DateTime.Now,
            string.Join(".", result.KeyPegs),
            string.Join("..", game.Code));
        await _efContext.AddMoveAsync(dataMove);

        if (result.KeyPegs.Count(s => s == black) == 4)
        {
            result = result with { Won = true };
            MasterMindGame efGame = new MasterMindGame(game.GameId, string.Join("..", game.Code), game.Name, DateTime.Now);
            await _efContext.AddGameAsync(efGame);
        }
        
        _logger.LogInformation("Received a move with {move}, returing {result}", move, result);
        return result;
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
