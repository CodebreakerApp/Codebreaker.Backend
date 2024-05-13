using System.Text.Json.Serialization;

namespace CodeBreaker.UserService.Models.Api;

internal class BeforeIncludingApplicationClaimsRequest
{
    public string Step { get; set; } = string.Empty;

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("ui_locales")]
    public string UiLocales { get; set; } = string.Empty;

    //
    // Claims:
    //
    public string ObjectId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    //public string DisplayName { get; set; } = string.Empty;

    public string GivenName { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    [JsonPropertyName("extension_69ecacfaed14473ba27f9937b5195afb_GamerName")]
    public string GamerName { get; set; } = string.Empty;
}
