using Microsoft.Extensions.Logging;

namespace CodeBreaker.Queuing.Common;

internal static partial class Logging
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug, Message = "Enqueued item: {message}")]
    public static partial void EnqueuedItem(this ILogger logger, string? message = null);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Debug, Message = "Dequeued item: {message}")]
    public static partial void DequeuedItem(this ILogger logger, string? message = null);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Debug, Message = "Dequeued empty item")]
    public static partial void DequeuedEmptyItem(this ILogger logger);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Debug, Message = "Removed item {messageId} from the queue")]
    public static partial void RemovedItem(this ILogger logger, string? messageId);
}
