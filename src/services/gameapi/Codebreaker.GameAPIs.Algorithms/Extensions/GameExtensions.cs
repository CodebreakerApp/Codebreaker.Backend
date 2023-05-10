using Codebreaker.GameAPIs.Models;

namespace Codebreaker.GameAPIs.Extensions;

public static class GameExtensions
{
    public static bool Ended<TField, TResult>(this ICalculatableGame<TField, TResult> game)
        where TResult: struct
        => game.EndTime is not null;

    public static void ApplyColorMove(this ICalculatableGame<ColorField, ColorResult> game, ICalculatableMove<ColorField, ColorResult> move)
    {
        if (game.Holes != move.GuessPegs.Count)
            throw new ArgumentException($"Invalid guess number {move.GuessPegs.Count} for {game.Holes} holes");

        if (move.GuessPegs.Any(guessPeg => !game.Fields.Contains(guessPeg)))
            throw new ArgumentException("The guess contains an invalid value");

        game.LastMoveNumber++;

        if (game.LastMoveNumber != move.MoveNumber)
            throw new ArgumentException($"Incorrect move number received {move.MoveNumber}");

        // Check black and white keyPegs
        List<ColorField> codesToCheck = new(game.Codes);
        List<ColorField> guessPegsToCheck = new(move.GuessPegs);
        int black = 0;
        List<string> whitePegs = new();

        // check black
        for (int i = 0; i < guessPegsToCheck.Count; i++)
            if (guessPegsToCheck[i] == codesToCheck[i])
            {
                black++;
                codesToCheck.RemoveAt(i);
                guessPegsToCheck.RemoveAt(i);
                i--;
            }

        // check white
        foreach (ColorField value in guessPegsToCheck)
        {
            // value not in code
            if (!codesToCheck.Contains(value))
                continue;

            // value peg was already added to the white pegs often enough
            // (max. the number in the codeToCheck)
            if (whitePegs.Count(x => x == value.Color) == codesToCheck.Count(x => x == value))
                continue;

            whitePegs.Add(value.Color);
        }

        ColorResult resultPegs = new(black, whitePegs.Count);

        if ((resultPegs.Correct + resultPegs.WrongPosition) > game.Holes)
            throw new InvalidOperationException("There are more keyPegs than holes"); // Should not be the case

        move.KeyPegs = resultPegs;

        game.Moves.Add(move);

        if (resultPegs.Correct == game.Holes)
        {
            game.Won = true;
        }

        if (resultPegs.Correct == game.Holes || game.Moves.Count >= game.MaxMoves)
        { 
            game.EndTime = DateTime.UtcNow;
            game.Duration = game.EndTime - game.StartTime;
        }
    }

    public static void ApplySimpleMove(this ICalculatableGame<ColorField, SimpleColorResult> game, ICalculatableMove<ColorField, SimpleColorResult> move)
    {
        if (game.Holes != move.GuessPegs.Count)
            throw new ArgumentException($"Invalid guess number {move.GuessPegs.Count} for {game.Holes} holes");

        if (move.GuessPegs.Any(guessPeg => !game.Fields.Contains(guessPeg)))
            throw new ArgumentException("The guess contains an invalid value");

        game.LastMoveNumber++;

        if (game.LastMoveNumber != move.MoveNumber)
            throw new ArgumentException($"Incorrect move number received {move.MoveNumber}");

        // Check black and white keyPegs
        List<ColorField> codesToCheck = new(game.Codes);
        List<ColorField> guessPegsToCheck = new(move.GuessPegs);

        var results = Enumerable.Repeat(ResultValue.Incorrect, 4).ToArray();

        for (int i = 0; i < results.Length; i++)
        {
            results[i] = ResultValue.Incorrect;
        }

        // check black
        for (int i = 0; i < game.Codes.Count; i++)
        {
            // check black
            if (guessPegsToCheck[i] == codesToCheck[i])
            {
                results[i] = ResultValue.CorrectPositionAndColor;
            }
            else // check white
            {
                if (codesToCheck.Contains(codesToCheck[i]) && results[i] == ResultValue.Incorrect)
                {
                    results[i] = ResultValue.CorrectColor;
                }
            }
        }

        game.Won = results.All(r => r == ResultValue.CorrectColor);

        if (game.Won || game.Moves.Count >= game.MaxMoves)
        {
            game.EndTime = DateTime.UtcNow;
            game.Duration = game.EndTime - game.StartTime;
        }

        move.KeyPegs = new SimpleColorResult(results);

        game.Moves.Add(move);
    }

    public static void ApplyShapeMove(this ICalculatableGame<ShapeAndColorField, ShapeAndColorResult> game, ICalculatableMove<ShapeAndColorField, ShapeAndColorResult> move)
    {
        if (game.Holes != move.GuessPegs.Count)
            throw new ArgumentException($"Invalid guess number {move.GuessPegs.Count} for {game.Holes} holes");

        // check for valid colors
        if (move.GuessPegs.Select(f => f.Color).Any(color => !game.Fields.Select(f => f.Color).Contains(color)))
            throw new ArgumentException("The guess contains an invalid color");

        // check for valid shapes
        if (move.GuessPegs.Select(f => f.Shape).Any(shape => !game.Fields.Select(c => c.Shape).Contains(shape)))
            throw new ArgumentException("The guess contains an invalid shape");

        game.LastMoveNumber++;

        if (game.LastMoveNumber != move.MoveNumber)
            throw new ArgumentException($"Incorrect move number received {move.MoveNumber}");

        // Check black and white keyPegs
        List<ShapeAndColorField> codesToCheck = new(game.Codes);
        List<ShapeAndColorField> guessPegsToCheck = new(move.GuessPegs);
        List<ShapeAndColorField> remainingCodesToCheck = new();
        List<ShapeAndColorField> remainingGuessPegsToCheck = new();

        byte black = 0;
        byte blue = 0;
        byte white = 0;

        // check black (correct color and shape)
        for (int i = 0; i < guessPegsToCheck.Count; i++)
        {
            if (guessPegsToCheck[i] == codesToCheck[i])
            {
                black++;
            }
            else
            {
                remainingCodesToCheck.Add(codesToCheck[i]);
                remainingGuessPegsToCheck.Add(guessPegsToCheck[i]);
            }
        }

        codesToCheck = remainingCodesToCheck;
        remainingCodesToCheck = new(codesToCheck);
        guessPegsToCheck = remainingGuessPegsToCheck;
        remainingGuessPegsToCheck = new();

        // check blue (correct shape and color on a wrong position)
        for (int i = 0; i < guessPegsToCheck.Count; i++)
        {
            ShapeAndColorField? codeField = codesToCheck.FirstOrDefault(c => c == guessPegsToCheck[i]);
            if (codeField is not null)
            {
                blue++;
                remainingCodesToCheck.Remove(codeField); // remove for the white check
            }
            else
            {
                remainingGuessPegsToCheck.Add(guessPegsToCheck[i]);  // add for the white check
            }
        }

        codesToCheck = remainingCodesToCheck;
        guessPegsToCheck = remainingGuessPegsToCheck;

        // check white (either the shape or the color is correct on a wrong position)
        for (int i = 0; i < guessPegsToCheck.Count; i++)
        {
            var colorCodes = codesToCheck.Select(c => c.Color).ToArray();
            var shapeCodes = codesToCheck.Select(c => c.Shape).ToArray();

            if (colorCodes.Contains(guessPegsToCheck[i].Color) || shapeCodes.Contains(guessPegsToCheck[i].Shape))
            {
                white++;
            }
        }

        ShapeAndColorResult resultPegs = new(black, blue, white);

        if ((resultPegs.Correct + resultPegs.WrongPosition + resultPegs.ColorOrShape) > game.Holes)
            throw new InvalidOperationException("There are more keyPegs than holes"); // Should not be the case

        move.KeyPegs = resultPegs;

        game.Moves.Add(move);

        if (resultPegs.Correct == game.Holes)
        {
            game.Won = true;
        }

        if (resultPegs.Correct == game.Holes || game.Moves.Count >= game.MaxMoves)
        {
            game.EndTime = DateTime.UtcNow;
            game.Duration = game.EndTime - game.StartTime;
        }
    }
}
