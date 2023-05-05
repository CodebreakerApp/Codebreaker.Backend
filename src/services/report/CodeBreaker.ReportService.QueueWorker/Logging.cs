using Microsoft.Extensions.Logging;

namespace CodeBreaker.ReportService.QueueWorker;

public static partial class Logging
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Background service starting")]
    public static partial void BackgroundServiceStarting(this ILogger logger);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Background service stopping")]
    public static partial void BackgroundServiceStopping(this ILogger logger);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Background service finished")]
    public static partial void BackgroundServiceFinished(this ILogger logger);

    [LoggerMessage(EventId = 4000, Level = LogLevel.Debug, Message = "No item in the queue {queueName}")]
    public static partial void NoItemInTheQueue(this ILogger logger, string? queueName = null);
}
