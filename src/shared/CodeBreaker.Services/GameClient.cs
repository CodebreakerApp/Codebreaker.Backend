using CodeBreaker.Shared.APIModels;

using System.Net.Http.Json;

namespace CodeBreaker.Services;

public class GameClient
{
    private readonly HttpClient _httpClient;

    public GameClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CreateGameResponse> StartGameAsync(string name)
    {
        CreateGameRequest request = new(name);
        var responseMessage = await _httpClient.PostAsJsonAsync("start/6x4", request);
        responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadFromJsonAsync<CreateGameResponse>();
        return response;
    }

    public async Task<(bool Completed, bool Won, string[] KeyPegs)> SetMoveAsync(string gameId, int moveNumber, params string[] colorNames)
    {
        MoveRequest moveRequest = new(gameId, moveNumber, colorNames);

        var responseMessage = await _httpClient.PostAsJsonAsync("move/6x4", moveRequest);
        responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadFromJsonAsync<MoveResponse>();
        return (response.Completed, response.Won, response.KeyPegs?.ToArray() ?? new string[0]);
    }
}
