namespace CodeBreaker.UserService.Models.Api;

internal class BeforeIncludingApplicationClaimsResponse
{
    public string Version { get; } = "1.0.0";

    public string Action { get; } = "Continue";

    //
    // Claims:
    //
    public string ObjectId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    //public string DisplayName { get; set; } = string.Empty;

    public string GivenName { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string GamerName { get; set; } = string.Empty;

    public string[] UserGroups { get; set; } = [];
}
