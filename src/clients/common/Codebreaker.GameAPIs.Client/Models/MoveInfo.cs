namespace Codebreaker.GameAPIs.Client.Models;

/// <summary>
/// Represents the move within a game <see cref="GameInfo"/ >with the guess pegs and key pegs.
/// </summary>
/// <param name="id">The unique identifier of the move. This is needed to reference the move.</param>
/// <param name="moveNumber">The move number for this move within the associated game.</param>
/// <param name="guessPegs">The guess pegs from the user for this move.</param>
/// <paramref name="keyPegs">The result from the analyzer for this move based on the associated game that contains the move.</param>
public class MoveInfo(Guid id, int moveNumber, string[] guessPegs, string[] keyPegs)
{
    /// <summary>
    /// Gets the unique identifier of the move.
    /// </summary>
    public Guid Id { get; private set; } = id;

    /// <summary>
    /// The move number for this move within the associated game.
    /// </summary>
    public int MoveNumber { get; private set; } = moveNumber;

    /// <summary>
    /// The guess pegs from the user for this move.
    /// </summary>
    public string[] GuessPegs { get; private set; } = guessPegs;

    /// <summary>
    /// The result from the analyzer for this move based on the associated game that contains the move.
    /// </summary>
    public string[] KeyPegs { get; private set; } = keyPegs;

    public override string ToString() => @$"{MoveNumber}.
        {string.Join('#', GuessPegs)} :
        {string.Join('#', KeyPegs)}";
}
