using CodeBreaker.Services.Authentication;
using CodeBreaker.Shared;

using Microsoft.Extensions.Logging;

using System.Net.Http.Json;

namespace CodeBreaker.Services;

public class GameClient : IGameClient, IGameReportClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly ILogger _logger;

    public GameClient(HttpClient httpClient, ILogger<GameClient> logger, IAuthService authService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _logger.LogInformation("Injected HttpClient with base address {uri} into GameClient", _httpClient.BaseAddress);
        _authService = authService;
    }

    public async Task<CreateGameResponse> StartGameAsync(string name)
    {
        await SetAuthentication();
        CreateGameRequest request = new(name);
        var responseMessage = await _httpClient.PostAsJsonAsync("start/6x4", request);
        responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadFromJsonAsync<CreateGameResponse>();
        return response;
    }

    public async Task<(bool Completed, bool Won, string[] KeyPegs)> SetMoveAsync(Guid gameId, int moveNumber, params string[] colorNames)
    {
        await SetAuthentication();
        MoveRequest moveRequest = new(gameId, moveNumber, colorNames);
        var responseMessage = await _httpClient.PostAsJsonAsync("move/6x4", moveRequest);
        responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadFromJsonAsync<MoveResponse>();
        return (response.Completed, response.Won, response.KeyPegs?.ToArray() ?? Array.Empty<string>());
    }

    public async Task<IEnumerable<GamesInfo>?> GetReportAsync(DateTime? date)
    {
        string requestUri = "/report";

        if (date is not null)
            requestUri = $"{requestUri}?date={date:yyyy-MM-dd}";

        await SetAuthentication();
        _logger.LogInformation("Calling Codebreaker with {uri}", requestUri);

        return await _httpClient.GetFromJsonAsync<IEnumerable<GamesInfo>>(requestUri);
    }

    public async Task<CodeBreakerGame?> GetDetailedReportAsync(Guid id)
    {
        await SetAuthentication();
        string requestUri = $"/reportdetail/{id}";

        _logger.LogInformation("Calling Codebreaker with {uri}", requestUri);

        return await _httpClient.GetFromJsonAsync<CodeBreakerGame?>(requestUri);
    }

    private async ValueTask SetAuthentication()
    {
        if (!_authService.IsAuthenticated)
            return;

        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", await GetAccessTokenAsync());
    }

    private async Task<string> GetAccessTokenAsync() =>
        (await _authService.AquireTokenAsync()).AccessToken;
}
