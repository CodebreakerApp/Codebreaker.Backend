namespace Codebreaker.Ranking.Extensions;

public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 10001,
        Level = LogLevel.Information,
        Message = "Processing event")]
    public static partial void ProcessingEvent(this ILogger logger);

    [LoggerMessage(
        EventId = 10002,
        Level = LogLevel.Warning,
        Message = "Game summary is empty after deserialization")]
    public static partial void GameSummaryIsEmpty(this ILogger logger);

    [LoggerMessage(
        EventId = 10003,
        Level = LogLevel.Information,
        Message = "Received game completion event for game {GameId}")]
    public static partial void ReceivedGameCompletionEvent(this ILogger logger, Guid gameId);

    [LoggerMessage(
        EventId = 10004,
        Level = LogLevel.Error,
        Message = "Error processing event: {Error}")]
    public static partial void ErrorProcessingEvent(this ILogger logger, Exception ex, string error);

    [LoggerMessage(
        EventId = 10005,
        Level = LogLevel.Error,
        Message = "{Error}")]
    public static partial void Error(this ILogger logger, Exception ex, string error);

    [LoggerMessage(
        EventId = 10006,
        Level = LogLevel.Error,
        Message = "Deserialized null GameSummary")]
    public static partial void DeserializedNullGameSummary(this ILogger logger);

    [LoggerMessage(
        EventId = 10007,
        Level = LogLevel.Warning,
        Message = "Consume exception: {Message}")]
    public static partial void ConsumeException(this ILogger logger, string message);

    [LoggerMessage(
        EventId = 10008,
        Level = LogLevel.Information,
        Message = "Processing was cancelled")]
    public static partial void ProcessingWasCancelled(this ILogger logger);
}