﻿namespace Codebreaker.GameAPIs.Client.Models;

/// <summary>
/// Represents the move within a game <see cref="GameInfo"/ >with the guess pegs and key pegs.
/// </summary>
/// <param name="id"/>The unique identifier of the move. This is needed to reference the move.
/// <param name="moveNumber"/>The move number for this move within the associated game.
public class MoveInfo(Guid id, int moveNumber)
{
    public Guid Id { get; private set; } = id;

    /// <summary>
    /// The move number for this move within the associated game.
    /// </summary>
    public int MoveNumber { get; private set; } = moveNumber;

    /// <summary>
    /// The guess pegs from the user for this move.
    /// </summary>
    public required string[] GuessPegs { get; init; }
    /// <summary>
    /// The result from the analyzer for this move based on the associated game that contains the move.
    /// </summary>
    public required string[] KeyPegs { get; init; }

    public override string ToString() => $"{MoveNumber}. " +
        $"{string.Join('#', GuessPegs)} : " +
        $"{string.Join('#', KeyPegs)}";
}
