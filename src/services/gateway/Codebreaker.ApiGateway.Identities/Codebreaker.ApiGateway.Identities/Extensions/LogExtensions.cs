namespace Codebreaker.ApiGateway.Identities.Extensions;

public static partial class LogExtensions
{
    [LoggerMessage(
        EventId = 30001,
        Level = LogLevel.Information,
        Message = "User with ID '{UserId}' asked for their personal data")]
    public static partial void UserRequestedPersonalData(this ILogger logger, string userId);
}