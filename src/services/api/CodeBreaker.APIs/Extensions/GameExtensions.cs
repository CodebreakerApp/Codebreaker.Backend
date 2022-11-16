using CodeBreaker.Shared.Models.Data;
using CodeBreaker.Shared.Models.Extensions;

namespace CodeBreaker.APIs.Extensions;

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
        List<string> codeToCheck = new(game.Code);
        List<string> guessPegsToCheck = new(move.GuessPegs);
        int black = 0;
        List<string> whitePegs = new();

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
        foreach (string value in guessPegsToCheck)
        {
            // value not in code
            if (!codeToCheck.Contains(value))
                continue;

            // value peg was already added to the white pegs often enough
            // (max. the number in the codeToCheck)
            if (whitePegs.Count(x => x == value) == codeToCheck.Count(x => x == value))
                continue;

            whitePegs.Add(value);
        }

        KeyPegs keyPegs = new(black, whitePegs.Count);

        if (keyPegs.Total > game.Type.Holes)
            throw new InvalidOperationException("Their are more keyPegs than holes"); // Should not be the case

        move.KeyPegs = keyPegs;

        game.Moves.Add(move);

        // all holes correct  OR  maxmoves reached
        if (keyPegs.Black == game.Type.Holes || game.Moves.Count >= game.Type.MaxMoves)
            game.End = DateTime.Now;
    }
}
