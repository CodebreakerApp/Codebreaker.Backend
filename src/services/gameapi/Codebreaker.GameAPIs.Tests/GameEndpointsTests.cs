using System.Net;
using System.Net.Http.Json;

using Codebreaker.GameAPIs.Models;

namespace Codebreaker.GameAPIs.Tests;

public class GameEndpointsTests
{
    [Fact]
    public async Task SetMoveWithInvalidMoveNumberShouldReturnBadRequest()
    {
        int moveNumber = 0;
        await using GamesApiApplication app = new();
        var client = app.CreateClient();
        CreateGameRequest request = new(GameType.Game6x4, "test");
        var response = await client.PostAsJsonAsync("/games", request);
        var gameReponse = await response.Content.ReadFromJsonAsync<CreateGameResponse>();

        if (gameReponse is null) Assert.Fail("gameResponse is null");

        UpdateGameRequest updateGameRequest = new(gameReponse.GameId, gameReponse.GameType, gameReponse.PlayerName, moveNumber)
        {
            GuessPegs = new string[] { "Red", "Red", "Red", "Red" }
        };

        string uri = $"/games/{updateGameRequest.GameId}";
        var updateGameResponse = await client.PatchAsJsonAsync(uri, updateGameRequest);

        Assert.Equal(HttpStatusCode.BadRequest, updateGameResponse.StatusCode);       
    }
}
