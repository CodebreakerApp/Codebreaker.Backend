using Azure.Identity;
using CodeBreaker.UserService.Options;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace CodeBreaker.UserService.Services;

internal class GamerNameService : IGamerNameService
{
    private readonly MicrosoftGraphOptions _microsoftGraphOptions;

    private readonly GamerNameSuggestionOptions _gamerNameSuggestionOptions;

    public GamerNameService(IOptions<MicrosoftGraphOptions> microsoftGraphOptions, IOptions<GamerNameSuggestionOptions> gamerNameSuggestionOptions)
    {
        _microsoftGraphOptions = microsoftGraphOptions.Value;
        _gamerNameSuggestionOptions = gamerNameSuggestionOptions.Value;
    }

    public async Task<bool> CheckGamerNameAsync(string gamerName)
    {
        if (string.IsNullOrWhiteSpace(gamerName)) return false;
        if (gamerName.Length < 5 || gamerName.Length > 50) return false;

        string[] scopes = new[] { "https://graph.microsoft.com/.default" };
        TokenCredentialOptions options = new() { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };
        ClientSecretCredential clientSecretCredential = new(_microsoftGraphOptions.TenantId, _microsoftGraphOptions.ClientId, _microsoftGraphOptions.ClientSecret, options);
        GraphServiceClient client = new(clientSecretCredential, scopes);
        var request = client.Users.Request()
            .Top(1)         // Basically unnecessary, because every gamerName should only exist once
            .Select("id")
            .Filter($"extension_dd21590c971e431494da34e2a8d47cce_GamerName eq '{gamerName}'"); // TODO: Check if the extensionKey is constant. Otherwise:  // await client.Applications["4f40c49c-be3d-4fcd-a413-e3a0a9036df6"].ExtensionProperties.Request().GetResponseAsync();
        var response = await request.GetResponseAsync();
        CheckGamerNameResult? result = await response.Content.ReadFromJsonAsync<CheckGamerNameResult>();
        return result?.Value.Length == 0;
    }

    public async IAsyncEnumerable<string> SuggestGamerNamesAsync(int count = 10)
    {
        (int, int, int)[] combinations = new(int, int, int)[count];
        string[] first = new[]
        {
            "cool",
            "smart",
            "genius",
            "red",
            "green",
            "blue",
            "yellow"
        };
        string[] second = new[]
        {
            "Beast",
            "Champ",
            "Tornado",
            "Wizard",
            "Boss",
            "KingKong",
            "Godzilla",
            "Sentinel",
            "Speed"
        };

        for (int i = 0; i < combinations.Length; i++)
        {
            var candidate = (first.GetRandomIndex(), second.GetRandomIndex(), Random.Shared.Next(_gamerNameSuggestionOptions.MinimumNumber, _gamerNameSuggestionOptions.MaximumNumber));

            if (combinations.Contains(candidate))
            {
                i--;
                continue;
            }

            combinations[i] = candidate;
            yield return $"{first[candidate.Item1]}-{second[candidate.Item2]}-{candidate.Item3}";
        }
    }

    private class CheckGamerNameResult
    {
        public CheckGamerNameResultItem[] Value { get; set; } = Array.Empty<CheckGamerNameResultItem>();

        public class CheckGamerNameResultItem
        {
            public string Id { get; set; } = string.Empty;
        }
    }
}

file static class ArrayExtensions
{
    public static TItem GetRandom<TItem>(this TItem[] array) =>
        array[Random.Shared.Next(0, array.Length)];

    public static int GetRandomIndex<TItem>(this TItem[] array) =>
        Random.Shared.Next(0, array.Length);
}
