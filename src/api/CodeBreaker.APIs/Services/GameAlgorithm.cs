using static CodeBreaker.Shared.CodeBreakerColors;

namespace CodeBreaker.APIs.Services;

// algorithm for the simple string type (which covers many types). When (color and shapes) games are implemented, this might change to a generic type, or the name of the class might change
internal class GameAlgorithm : IGameAlgorithm
{
    private readonly ILogger _logger;

    public GameAlgorithm(ILogger<GameAlgorithm> logger)
    {
        _logger = logger;
    }

    public (GameMoveResult Result, CodeBreakerGame DataGame, CodeBreakerGameMove? Move) SetMove(Game game, GameMove guess)
    {
        if (game.Holes != guess.GuessPegs.Count)
        {
            _logger.MoveWithInvalidGuesses(guess.GuessPegs.Count, game.Holes);
            throw new ArgumentException($"Invalid guess number {guess.GuessPegs.Count} for {game.Holes} holes");
        }

        if (guess.GuessPegs.Except(game.ColorList).Any())
        {
            string invalidGuesses = string.Join('_', guess.GuessPegs.Except(game.ColorList).ToArray());
            _logger.MoveWithInvalidGuessValues(invalidGuesses);
            throw new ArgumentException($"Invalid guess values {invalidGuesses}");
        }

        GameMoveResult result = new(guess.GameId, guess.MoveNumber);

        // TODO: check for mismatch of moves! if (guess.MoveNumber != game.codd)

        CodeBreakerGame dataGame = game.ToDataGame();

        if (guess.MoveNumber > 12)
        {
            result = result with { Completed = true };  // completed, but not won
            return (result, dataGame, null);
        }

        List<string> codes = new(game.Code); // copy the codes from the game to check the codes that are already calculated
        List<string> guesses = new(guess.GuessPegs); // copy the guesses to check the moves that are already calculated
        List<int> blackHits = new(); // correct positions where blacks are found
        List<string> keyPegs = new(); // the final information for the keys, black and white pegs are added

        // first check for the correct position
        for (int i = 0; i < game.Holes; i++)
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
            Guid.NewGuid(),
            game.GameId,
            guess.MoveNumber,
            string.Join("..", guess.GuessPegs),
            DateTime.Now,
            string.Join(".", result.KeyPegs),
            string.Join("..", game.Code)); ;

        // write the complete game to the data store if it finished
        if (result.KeyPegs.Count(s => s == Black) == 4)
        {
            result = result with { Won = true, Completed = true };
            dataGame = game.ToDataGame();
        }

        _logger.SetMove(guess.ToString(), result.ToString());
        return (result, dataGame, dataMove);
    }

    private List<string> GetWhiteKeyPegs(List<string> codes, List<string> moves, List<string> keyPegs)
    {
        List<KeyPegWithFlag> tempCode = new(codes.Select(c => new KeyPegWithFlag(c, false)));
        List<KeyPegWithFlag> tempMoves = new(moves.Select(m => new KeyPegWithFlag(m, false)));

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
