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
            throw new ArgumentOutOfRangeException("The guess contains an invalid value");

        // Check MoveCount
        //if (game.Moves.Any(m => m.MoveNumber >= move.MoveNumber))
        //    throw new ArgumentOutOfRangeException("The order of the moveNumbers is not correct"); // TODO other exception type
        move.MoveNumber = game.GetLastMoveOrDefault()?.MoveNumber + 1 ?? 0;

        // Check black and white keyPegs
        KeyPegs keyPegs = new KeyPegs();

        for (int i = 0; i < move.GuessPegs.Count; i++)
            if (move.GuessPegs[i] == game.Code[i])
                keyPegs.Black++;
            else if (game.Code.Contains(move.GuessPegs[i]))
                keyPegs.White++;

        move.KeyPegs = keyPegs;
        game.Moves.Add(move);

        if (keyPegs.Black + keyPegs.White > game.Type.Holes)
            throw new Exception("Their are more keyPegs than holes"); // Should not be the case
    }
}
