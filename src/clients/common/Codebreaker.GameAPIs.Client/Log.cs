namespace Codebreaker.GameAPIs.Client;

internal static partial class Log
{
    [LoggerMessage(2000, LogLevel.Information, "GetGameAsync game {id} not found: {error}", EventName = "GetGame")]
    public static partial void GetGameNotFound(this ILogger logger, Guid id, string error);

    [LoggerMessage(5001, LogLevel.Error, "GetGameAsync error {message}", EventName = "GetGameError")]
    public static partial void GetGameError(this ILogger logger, string message, Exception ex);

    [LoggerMessage(5002, LogLevel.Error, "GetGamesAsync error {message}", EventName ="GetGamesError")]
    public static partial void GetGamesError(this ILogger logger, string message, Exception ex);

    [LoggerMessage(5003, LogLevel.Error, "StartGameAsync error {message}", EventName ="StartGameError")]
    public static partial void StartGameError(this ILogger logger, string message, Exception ex);

    [LoggerMessage(5004, LogLevel.Error, "SetMoveAsync error {message}", EventName = "SetMoveError")]
    public static partial void SetMoveError(this ILogger logger, string message, Exception ex);

    [LoggerMessage(8001, LogLevel.Information, "Game {gameId} created", EventName = "GameCreated")]
    public static partial void GameCreated(this ILogger logger, string gameId);

    [LoggerMessage(8002, LogLevel.Information, "Move {moveNumber} for game {gameId} set", EventName = "MoveSet")]
    public static partial void MoveSet(this ILogger logger, string gameId, int moveNumber);

    [LoggerMessage(8003, LogLevel.Information, "Game {gameId} information received, ended {ended}, number moves {moveNumber}", EventName = "GameReceived")]
    public static partial void GameReceived(this ILogger logger, string gameId, bool ended, int moveNumber);

    [LoggerMessage(8004, LogLevel.Information, "With query {query}, {number} games received", EventName = "GamesReceived")]
    public static partial void GamesReceived(this ILogger logger, string query, int number);
}
