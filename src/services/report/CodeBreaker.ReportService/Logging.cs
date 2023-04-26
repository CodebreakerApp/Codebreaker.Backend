namespace CodeBreaker.ReportService;

public static partial class Logging
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "{serviceName} starting")]
    public static partial void BackgroundServiceStarting(this ILogger logger, string serviceName);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "{serviceName} stopping")]
    public static partial void BackgroundServiceStopping(this ILogger logger, string serviceName);

    [LoggerMessage(EventId = 4000, Level = LogLevel.Debug, Message = "No item in the queue {queueName}")]
    public static partial void NoItemInTheQueue(this ILogger logger, string? queueName = null);
}
