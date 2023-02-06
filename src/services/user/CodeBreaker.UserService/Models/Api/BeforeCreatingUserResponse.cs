namespace CodeBreaker.UserService.Models.Api;

internal abstract class BeforeCreatingUserResponse
{
    public BeforeCreatingUserResponse(string action)
    {
        Action = action;
    }

    public string Version { get; } = "1.0.0";

    public string Action { get; private init; }
}

internal class BeforeCreatingUserSuccessResponse : BeforeCreatingUserResponse
{
    public BeforeCreatingUserSuccessResponse() : base("Continue")
    {
    }

    // Is it possible to overwrite claims by specifying them here. e.g. capitalize gamer name
    public string? Email { get; set; }

    public string? DisplayName { get; set; }

    public string? GivenName { get; set; }

    public string? Surname { get; set; }

    public string? Extension_dd21590c971e431494da34e2a8d47cce_GamerName { get; set; }
}

internal class BeforeCreatingUserBlockingResponse : BeforeCreatingUserResponse
{
    public BeforeCreatingUserBlockingResponse(string userMessage) : base("ShowBlockPage")
    {
        UserMessage = userMessage;
    }

    public string UserMessage { get; init; }
}

internal class BeforeCreatingUserValidationErrorResponse : BeforeCreatingUserResponse
{
    public BeforeCreatingUserValidationErrorResponse(string userMessage) : base("ValidationError")
    {
        UserMessage = userMessage;
    }

    public int Status { get; } = 400;

    public string UserMessage { get; init; }
}
