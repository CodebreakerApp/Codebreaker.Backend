namespace Codebreaker.GameAPIs.Client;

/// <summary>
/// Defines methods for interacting with games, including retrieving game information, managing game state,  and
/// performing player actions.
/// </summary>
/// <remarks>This interface provides functionality for querying game details, starting new games, making moves, 
/// and canceling games. It is designed to support various game types and player interactions.</remarks>
public interface IGamesClient
{
    /// <summary>
    /// Retrieves information about a game by its unique identifier.
    /// </summary>
    /// <remarks>Use this method to retrieve detailed information about a specific game. If the game does not
    /// exist, the method returns <see langword="null"/>. Ensure that the <paramref name="id"/> parameter is valid
    /// before calling this method.</remarks>
    /// <param name="id">The unique identifier of the game to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see
    /// cref="GameInfo"/> object with details about the game if found; otherwise, <see langword="null"/>.</returns>
    Task<GameInfo?> GetGameAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a collection of games based on the specified query criteria.
    /// </summary>
    /// <remarks>Use this method to retrieve game information based on specific filters, such as genre,
    /// release date, or platform. The query object defines the criteria for filtering the games.</remarks>
    /// <param name="query">The query parameters used to filter and retrieve the games. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will be canceled if the token is triggered.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of  <see
    /// cref="GameInfo"/> objects that match the query criteria. If no games match, the collection will be empty.</returns>
    Task<IEnumerable<GameInfo>> GetGamesAsync(GamesQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a player's move in the specified game and returns the results of the move.
    /// </summary>
    /// <param name="id">The unique identifier of the game session.</param>
    /// <param name="playerName">The name of the player making the move.</param>
    /// <param name="gameType">The type of game being played.</param>
    /// <param name="moveNumber">The sequential number of the move being made.</param>
    /// <param name="guessPegs">An array of strings representing the player's guess for the current move.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is a tuple containing: <list type="bullet">
    /// <item><description>An array of strings representing the results of the move.</description></item>
    /// <item><description>A boolean indicating whether the game has ended.</description></item> <item><description>A
    /// boolean indicating whether the move resulted in a victory.</description></item> </list></returns>
    Task<(string[] Results, bool Ended, bool IsVictory)> SetMoveAsync(Guid id, string playerName, GameType gameType, int moveNumber, string[] guessPegs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a new game asynchronously and returns the game details.
    /// </summary>
    /// <remarks>Use this method to initialize a new game session. The returned game details include the
    /// configuration and constraints for the game, which can be used to guide gameplay.</remarks>
    /// <param name="gameType">The type of game to start. This determines the rules and configuration of the game.</param>
    /// <param name="playerName">The name of the player starting the game. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is a tuple containing: <list type="bullet">
    /// <item><description><see cref="Guid"/> Id: The unique identifier for the newly created game.</description></item>
    /// <item><description><see cref="int"/> NumberCodes: The number of codes required to solve the
    /// game.</description></item> <item><description><see cref="int"/> MaxMoves: The maximum number of moves allowed in
    /// the game.</description></item> <item><description><see cref="IDictionary{TKey, TValue}"/> FieldValues: A
    /// dictionary containing field names as keys and their possible values as arrays of strings.</description></item>
    /// </list></returns>
    Task<(Guid Id, int NumberCodes, int MaxMoves, IDictionary<string, string[]> FieldValues)> StartGameAsync(GameType gameType, string playerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an ongoing game session.
    /// </summary>
    /// <remarks>This method cancels the specified game session and may notify other players or systems
    /// depending on the game type. Ensure the <paramref name="id"/> corresponds to an active game session and that the
    /// caller has the necessary permissions to cancel the game.</remarks>
    /// <param name="id">The unique identifier of the game session to cancel.</param>
    /// <param name="playerName">The name of the player requesting the cancellation. Must not be null or empty.</param>
    /// <param name="gameType">The type of game being canceled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Obsolete(message: "Use RevealGameAsync instead", error: false)]
    Task CancelGameAsync(Guid id, string playerName, GameType gameType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels and reveals the details of a game based on the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier of the game to reveal.</param>
    /// <param name="playerName">The name of the player requesting the game details. Cannot be null or empty.</param>
    /// <param name="gameType">The type of the game to reveal.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
    /// <returns>A <see cref="GameInfo"/> object containing the details of the revealed game.</returns>
    Task<GameInfo> RevealGameAsync(Guid id, string playerName, GameType gameType, CancellationToken cancellationToken = default);
}