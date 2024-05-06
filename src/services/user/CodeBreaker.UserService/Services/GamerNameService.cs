using System.Runtime.CompilerServices;
using System.Text.Json;
using Azure.Identity;
using Azure.Storage.Blobs;
using CodeBreaker.Shared.Models.Users;
using CodeBreaker.UserService.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace CodeBreaker.UserService.Services;

internal class GamerNameService : IGamerNameService
{
    private readonly GamerNameCheckOptions _gamerNameCheckOptions;

    private readonly GamerNameSuggestionOptions _gamerNameSuggestionOptions;

    private readonly BlobServiceClient _blobServiceClient;

    private readonly IDistributedCache _cache;

    public GamerNameService(IOptions<GamerNameCheckOptions> gamerNameCheckOptions, IOptions<GamerNameSuggestionOptions> gamerNameSuggestionOptions, BlobServiceClient blobServiceClient, IDistributedCache cache)
    {
        _gamerNameCheckOptions = gamerNameCheckOptions.Value;
        _gamerNameSuggestionOptions = gamerNameSuggestionOptions.Value;
        _blobServiceClient = blobServiceClient;
        _cache = cache;
    }

    public async Task<bool> IsGamerNameTakenAsync(string gamerName, CancellationToken cancellationToken = default)
    {
        string[] scopes = ["https://graph.microsoft.com/.default"];
        TokenCredentialOptions options = new() { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };
        ClientSecretCredential clientSecretCredential = new(_gamerNameCheckOptions.TenantId, _gamerNameCheckOptions.ClientId, _gamerNameCheckOptions.ClientSecret, options);
        GraphServiceClient client = new(clientSecretCredential, scopes);
        var request = client.Users.Request()
            .Top(1)         // Basically unnecessary, because every gamerName should only exist once
            .Select("id")
            .Filter($"{_gamerNameCheckOptions.GamerNameAttributeKey} eq '{gamerName}'"); // TODO: Check if the extensionKey is constant. Otherwise:  // await client.Applications["4f40c49c-be3d-4fcd-a413-e3a0a9036df6"].ExtensionProperties.Request().GetResponseAsync();
        var response = await request.GetResponseAsync(cancellationToken);
        CheckGamerNameResult? result = await response.Content.ReadFromJsonAsync<CheckGamerNameResult>(cancellationToken: cancellationToken);
        return result?.Value.Length == 0;
    }

    public async IAsyncEnumerable<string> SuggestGamerNamesAsync(int count = 10, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const string CACHEKEY = "gamerNameParts";
        byte[]? cachedParts = await _cache.GetAsync(CACHEKEY, cancellationToken);
        GamerNameParts? parts = null;

        if (cachedParts != null)
            parts = JsonSerializer.Deserialize<GamerNameParts>(cachedParts, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        if (parts == null)
        {
            BlobClient blobClient = _blobServiceClient.GetBlobContainerClient("user-service").GetBlobClient("gamer-name-parts.json");
            Stream readStream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
            parts = await JsonSerializer.DeserializeAsync<GamerNameParts>(readStream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }, cancellationToken) ?? throw new ArgumentNullException("Error reading parts for the gamerName");
            await _cache.SetAsync(CACHEKEY, JsonSerializer.SerializeToUtf8Bytes<GamerNameParts>(parts), new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) });
        }

        if (parts.First.Length == 0 || parts.Second.Length == 0)
            throw new ArgumentOutOfRangeException("The word lists for the gamerName parts are empty.");

        (int, int, int)[] combinations = new (int, int, int)[count];

        for (int i = 0; i < combinations.Length; i++)
        {
            var candidate = (parts.First.GetRandomIndex(), parts.Second.GetRandomIndex(), Random.Shared.Next(_gamerNameSuggestionOptions.MinimumNumber, _gamerNameSuggestionOptions.MaximumNumber));

            if (combinations.Contains(candidate))
            {
                i--;
                continue;
            }

            combinations[i] = candidate;

            var first = parts.First[candidate.Item1].ToCharArray();
            first[0] = char.ToUpper(first[0]);
            yield return $"{new string(first)}{parts.Second[candidate.Item2]}{candidate.Item3}";
        }
    }

    private class CheckGamerNameResult
    {
        public CheckGamerNameResultItem[] Value { get; set; } = [];

        public class CheckGamerNameResultItem
        {
            public string Id { get; set; } = string.Empty;
        }
    }
}

file static class ArrayExtensions
{
    public static int GetRandomIndex<TItem>(this TItem[] array) =>
        Random.Shared.Next(0, array.Length);
}
