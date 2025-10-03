using Microsoft.Extensions.Logging;

namespace Codebreaker.Identity.Tests.Extensions;

public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 35001,
        Level = LogLevel.Information,
        Message = "Creating mock anonymous user: {UserName}")]
    public static partial void CreatingMockAnonymousUser(this ILogger logger, string userName);

    [LoggerMessage(
        EventId = 35002,
        Level = LogLevel.Information,
        Message = "Deleting stale anonymous users")]
    public static partial void DeletingStaleAnonymousUsers(this ILogger logger);

    [LoggerMessage(
        EventId = 35003,
        Level = LogLevel.Information,
        Message = "Deleted {Count} stale anonymous users")]
    public static partial void DeletedStaleAnonymousUsers(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 35004,
        Level = LogLevel.Information,
        Message = "Promoting mock anonymous user: {UserId}")]
    public static partial void PromotingMockAnonymousUser(this ILogger logger, string userId);
}