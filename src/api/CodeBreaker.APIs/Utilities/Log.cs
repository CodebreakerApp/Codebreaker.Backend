namespace CodeBreaker.APIs;

public static partial class Log
{
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Error,
        Message = "{message}")]
    public static partial void Error(this ILogger logger, Exception ex, string message);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Error,
        Message = "Move number does not match - received: `{receivedMoveNumber}`, expected: `{serverSideMoveNumber}`")]
    public static partial void MismatchMoveNumber(this ILogger logger, int receivedMoveNumber, int serverSideMoveNumber);
        
    [LoggerMessage(
        EventId = 4000,
        Level = LogLevel.Information,
        Message = "Started a game `{game}`")]
    public static partial void GameStarted(this ILogger logger, string game);

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Information,
        Message = "Game `{game}` has ended")]
    public static partial void GameEnded(this ILogger logger, string game);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Information,
        Message = "Received a move with {move}, returing {result}")]
    public static partial void SetMove(this ILogger logger, string move, string result);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Information,
        Message = "New game cached, currently {count} games active")]
    public static partial void GameCached(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Warning,
        Message = "Game {id} not retrieved from cache")]
    public static partial void GameNotCached(this ILogger logger, string id);

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Warning,
        Message = "Game {id} not found in database")]
    public static partial void GameIdNotFound(this ILogger logger, string id);

    [LoggerMessage(
        EventId = 4006,
        Level = LogLevel.Information,
        Message = "Requesting game report for {date}")]
    public static partial void GameReport(this ILogger logger, string date);

    [LoggerMessage(
        EventId = 4007,
        Level = LogLevel.Information,
        Message = "Requesting detailed game report for {id}")]
    public static partial void DetailedGameReport(this ILogger logger, string id);
}
