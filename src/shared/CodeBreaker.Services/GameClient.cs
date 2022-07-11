﻿using CodeBreaker.Shared;
using CodeBreaker.Shared.APIModels;

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

    public async Task<IEnumerable<GamesInfo>?> GetReportAsync(DateTime? date)
    {
        string requestUri = "/report";
        if (date is not null)
        {
            requestUri = $"{requestUri}?date={date.Value.ToString("yyyy-MM-dd")}";
        }
        _logger.LogInformation("Calling Codebreaker with {uri}", requestUri);

        return await _httpClient.GetFromJsonAsync<IEnumerable<GamesInfo>>(requestUri);
    }

    public async Task<CodeBreakerGame?> GetDetailedReportAsync(string id)
    {
        string requestUri = $"/reportdetail/{id}";

        _logger.LogInformation("Calling Codebreaker with {uri}", requestUri);

        return await _httpClient.GetFromJsonAsync<CodeBreakerGame?>(requestUri);
    }
}