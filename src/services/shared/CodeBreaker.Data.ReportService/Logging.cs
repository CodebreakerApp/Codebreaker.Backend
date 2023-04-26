using Microsoft.Extensions.Logging;

namespace CodeBreaker.Data.Common;

public static partial class Logging
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Created entity \"{entityName}\" with id {entityId}")]
    public static partial void EntityCreated(this ILogger logger, string entityName, string entityId);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Updated entity \"{entityName}\" with id {entityId}")]
    public static partial void EntityUpdated(this ILogger logger, string entityName, string entityId);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Deleted entity \"{entityName}\" with id {entityId}")]
    public static partial void EntityDeleted(this ILogger logger, string entityName, string entityId);
}
