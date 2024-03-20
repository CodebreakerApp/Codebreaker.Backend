namespace Codebreaker.GameAPIs.Client.Models;

/// <summary>
/// Set a move for a game
/// </summary>
/// <param name="Id">The unique game identifier</param>
/// <param name="GameType">The game type with one of the <see cref="GameType"/>enum values</param>
/// <param name="PlayerName">The name of the player making the move</param>
/// <param name="MoveNumber">The number of the move</param>
/// <param name="End">Indicates whether the game has ended</param>
internal record class UpdateGameRequest(
    Guid Id,
    GameType GameType,
    string PlayerName,
    int MoveNumber,
    bool End = false)
{
    /// <summary>
    /// Gets or sets the guess pegs for the move
    /// </summary>
    public string[]? GuessPegs { get; set; }
}
