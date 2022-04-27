namespace CodeBreaker.APIs;

public static partial class Log
{
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Error,
        Message = "{message}")]
    public static partial void Error(this ILogger logger, Exception ex, string message);
    
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
}
