using CodeBreaker.Shared.Models.Data;
using CodeBreaker.Shared.Models.Extensions;

namespace CodeBreaker.Data.Extensions;

internal static class GameExtensions
{
    public static void ApplyMove(this Game game, Move move)
    {
        if (game.Type.Holes != move.GuessPegs.Count)
            throw new ArgumentException($"Invalid guess number {move.GuessPegs.Count} for {game.Type.Holes} holes");

        if (move.GuessPegs.Any(guessPeg => !game.Type.Fields.Contains(guessPeg)))
            throw new ArgumentException("The guess contains an invalid value");

        // Change MoveCount
        move.MoveNumber = game.GetLastMoveOrDefault()?.MoveNumber + 1 ?? 0;

        // Check black and white keyPegs
        var codeToCheck = new List<string>(game.Code);
        var guessPegsToCheck = new List<string>(move.GuessPegs);
        int black = 0;
        var whitePegs = new List<string>();

        // check black
        for (int i = 0; i < guessPegsToCheck.Count; i++)
            if (guessPegsToCheck[i] == codeToCheck[i])
            {
                black++;
                codeToCheck.RemoveAt(i);
                guessPegsToCheck.RemoveAt(i);
                i--;
            }

        // check white
        for (int i = 0; i < guessPegsToCheck.Count; i++)
        {
            string value = guessPegsToCheck[i];

            // value not in code
            if (!codeToCheck.Contains(value))
                continue;

            // value peg was already checked
            if (whitePegs.Contains(value))
                continue;

            whitePegs.Add(value);
        }

        var keyPegs = new KeyPegs(black, whitePegs.Count);

        if (keyPegs.Total > game.Type.Holes)
            throw new InvalidOperationException("Their are more keyPegs than holes"); // Should not be the case

        move.KeyPegs = keyPegs;

        game.Moves.Add(move);

        // all holes correct  OR  maxmoves reached
        if (keyPegs.Black == game.Type.Holes || game.Moves.Count >= game.Type.MaxMoves)
            game.End = DateTime.Now;
    }
}
