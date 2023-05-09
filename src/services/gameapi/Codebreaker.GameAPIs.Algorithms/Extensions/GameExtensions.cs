using System.Reflection;
using System.Runtime.CompilerServices;

using Codebreaker.GameAPIs.Models;

[assembly: InternalsVisibleTo("Codebreaker.GameAPIs.Algorithms.Tests")]

namespace Codebreaker.GameAPIs.Extensions;



public interface ICalculatableGame<TField, TResult>
{
    int Holes { get; }
    int MaxMoves { get; }
    DateTime EndTime { get; set; }
    bool Won { get; set; }
    IEnumerable<TField> Fields { get; }
    ICollection<TField> Codes { get; init; }
    ICollection<ICalculatableMove<TField, TResult>> Moves { get; }
}

public interface ICalculatableMove<TField, TResult>
{
    int MoveNumber { get; set; }
    ICollection<TField> GuessPegs { get; }
    TResult KeyPegs { get; set; }
}

internal static class GameExtensions
{
    public static void ApplyMove(this ICalculatableGame<ColorField, ColorResult> game, ICalculatableMove<ColorField, ColorResult> move)
    {
        if (game.Holes != move.GuessPegs.Count)
            throw new ArgumentException($"Invalid guess number {move.GuessPegs.Count} for {game.Holes} holes");

        if (move.GuessPegs.Any(guessPeg => !game.Fields.Contains(guessPeg)))
            throw new ArgumentException("The guess contains an invalid value");

        // Change MoveCount
        move.MoveNumber = game.Moves.LastOrDefault()?.MoveNumber + 1 ?? 0;

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
        }
    }
}
