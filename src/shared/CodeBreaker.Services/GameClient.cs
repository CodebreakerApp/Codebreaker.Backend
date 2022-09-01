using CodeBreaker.Shared.Models.Api;
using CodeBreaker.Shared.Models.Data;
using Microsoft.Extensions.Logging;

using System.Net.Http.Json;

namespace CodeBreaker.Services;

public class GameClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public GameClient(HttpClient httpClient, ILogger<GameClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _logger.LogInformation("Injected HttpClient with base address {uri} into GameClient", _httpClient.BaseAddress);
    }

    public async Task<CreateGameResponse> StartGameAsync(string username, string gameType)
{
        CreateGameRequest request = new(username, gameType);
        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync($"/games", request);
        responseMessage.EnsureSuccessStatusCode();
        return await responseMessage.Content.ReadFromJsonAsync<CreateGameResponse>();
    }

    public async Task<CreateMoveResponse> SetMoveAsync(Guid gameId, params string[] colorNames)
    {
        CreateMoveRequest request = new CreateMoveRequest(colorNames.ToList());
        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync($"/games/{gameId}/moves", request);
        responseMessage.EnsureSuccessStatusCode();
        return await responseMessage.Content.ReadFromJsonAsync<CreateMoveResponse>();
    }

    public async Task<IEnumerable<Game>> GetReportAsync(DateTime? date)
    {
        string requestUri = "/games";

        if (date is null)
            date = DateTime.Now;

        requestUri = $"{requestUri}?date={date.Value.ToString("yyyy-MM-dd")}";
        _logger.LogInformation("Calling Codebreaker with {uri}", requestUri);

        GetGamesResponse response = await _httpClient.GetFromJsonAsync<GetGamesResponse>(requestUri);
        return response.Games.Select(g => g.ToModel());
    }

    public async Task<Game?> GetDetailedReportAsync(Guid id)
    {
        string requestUri = $"/games/{id}";
        _logger.LogInformation("Calling Codebreaker with {uri}", requestUri);
        HttpResponseMessage responseMessage = await _httpClient.GetAsync(requestUri);
        responseMessage.EnsureSuccessStatusCode();
        GetGameResponse response = await responseMessage.Content.ReadFromJsonAsync<GetGameResponse>();
        return response.Game.ToModel();
    }
}
