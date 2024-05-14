using System.Text.Json.Serialization;

namespace CodeBreaker.UserService.Models.Api;

internal class BeforeCreatingUserRequest
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

    public string GivenName { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    [JsonPropertyName("extension_dd21590c971e431494da34e2a8d47cce_GamerName")]     // The user-attribute key for the gamer name, in the codebreaker3000 directory.
    public string GamerName { get; set; } = string.Empty;

    // This is a workaround for the fact that the gamer name is stored in two different directories.
    // This property is only used to deserialize the gamer name from the codebreakertest directory.
    // The value is then copied to the GamerName property.
    [JsonPropertyName("extension_69ecacfaed14473ba27f9937b5195afb_GamerName")]   // The user-attribute key for the gamer name, in the codebreakertest directory.
    private string? GamerName2
    {
        set
        {
            if (string.IsNullOrEmpty(value))
                return;

            GamerName = value;
        }
    }
}
