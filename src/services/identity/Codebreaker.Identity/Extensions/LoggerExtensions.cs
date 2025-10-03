using Microsoft.Extensions.Logging;

namespace Codebreaker.Identity.Extensions;

public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 30001,
        Level = LogLevel.Information,
        Message = "Creating anonymous user with name {UserName}")]
    public static partial void CreatingAnonymousUser(this ILogger logger, string userName);

    [LoggerMessage(
        EventId = 30002,
        Level = LogLevel.Error,
        Message = "Failed to create anonymous user {UserName}: {Message}")]
    public static partial void FailedToCreateAnonymousUser(this ILogger logger, Exception ex, string userName, string message);

    [LoggerMessage(
        EventId = 30003,
        Level = LogLevel.Information,
        Message = "Deleting anonymous users that haven't logged in for at least three months")]
    public static partial void DeletingStaleAnonymousUsers(this ILogger logger);

    [LoggerMessage(
        EventId = 30004,
        Level = LogLevel.Information,
        Message = "No anonymous users found")]
    public static partial void NoAnonymousUsersFound(this ILogger logger);

    [LoggerMessage(
        EventId = 30005,
        Level = LogLevel.Information,
        Message = "Deleted anonymous user: {UserId}, {DisplayName}")]
    public static partial void DeletedAnonymousUser(this ILogger logger, string userId, string? displayName);

    [LoggerMessage(
        EventId = 30006,
        Level = LogLevel.Information,
        Message = "Deleted {Count} anonymous users")]
    public static partial void DeletedAnonymousUsers(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 30007,
        Level = LogLevel.Error,
        Message = "Failed to delete anonymous users: {Message}")]
    public static partial void FailedToDeleteAnonymousUsers(this ILogger logger, Exception ex, string message);

    [LoggerMessage(
        EventId = 30008,
        Level = LogLevel.Information,
        Message = "Promoting anonymous user {UserId} to registered user")]
    public static partial void PromotingAnonymousUser(this ILogger logger, string userId);

    [LoggerMessage(
        EventId = 30009,
        Level = LogLevel.Error,
        Message = "Failed to promote anonymous user {UserId}: {Message}")]
    public static partial void FailedToPromoteAnonymousUser(this ILogger logger, Exception ex, string userId, string message);
}