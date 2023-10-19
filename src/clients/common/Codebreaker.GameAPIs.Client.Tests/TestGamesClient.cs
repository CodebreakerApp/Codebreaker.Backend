using Microsoft.Extensions.Logging.Abstractions;

namespace Codebreaker.GameAPIs.Client.Tests;

public class TestGamesClient(ITestOutputHelper outputHelper)
{
    private readonly ITestOutputHelper _outputHelper = outputHelper;

    [Fact]
    public async Task TestStartGame6x4Async()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x[It.IsAny<string>()]).Returns("http://localhost:5000");

        Mock<HttpMessageHandler> handlerMock = new (MockBehavior.Strict);
        string returnMessage = """
        {
            "gameId": "af8dd39f-6e16-41ef-9155-dcd3cf081e87",
            "gameType": "Game6x4",
            "playerName": "test",
            "numberCodes": 4,
            "maxMoves": 12,
            "fieldValues": {
                "colors": [
                    "Red",
                    "Green",
                    "Blue",
                    "Yellow",
                    "Purple",
                    "Orange"
                ]
            }
        }
        """;
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(returnMessage)
            }).Verifiable();

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new System.Uri(configMock.Object["GameAPIs"] ?? throw new InvalidOperationException())
        };
       
        var gamesClient = new GamesClient(httpClient, NullLogger<GamesClient>.Instance);

        // Act
        var response = await gamesClient.StartGameAsync(Models.GameType.Game6x4, "test");

        // Assert
        Assert.Equal(4, response.NumberCodes);
        Assert.Equal(12, response.MaxMoves);
        Assert.Single(response.FieldValues.Keys);
        Assert.Equal("colors", response.FieldValues.Keys.First());
        Assert.Equal(6, response.FieldValues["colors"].Length);

        handlerMock.Protected().Verify(
        "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }
}
