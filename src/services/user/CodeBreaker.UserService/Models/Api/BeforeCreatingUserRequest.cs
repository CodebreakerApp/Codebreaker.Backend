namespace CodeBreaker.UserService.Models.Api;

internal class BeforeCreatingUserRequest
{
    public string Step { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string UiLocales { get; set; } = string.Empty;

    //
    // Claims:
    //
    public string ObjectId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string GivenName { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string Extension_dd21590c971e431494da34e2a8d47cce_GamerName { get; set; } = string.Empty;
}
