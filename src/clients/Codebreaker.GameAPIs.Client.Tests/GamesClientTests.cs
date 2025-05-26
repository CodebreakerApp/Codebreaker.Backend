using Codebreaker.GameAPIs.Client.Models;
using System.Diagnostics;
using System.Text;

namespace Codebreaker.GameAPIs.Client.Tests;

public class GamesClientTests(ITestOutputHelper? testOutputHelper = null)
{
    [Fact]
    public async Task StartGameAsync_Should_ReturnResults()
    {
        // Arrange
        (var httpClient, var handlerMock) = GetHttpClientReturningACreatedGameSkeleton();

        GamesClient gamesClient = new(httpClient, NullLogger<GamesClient>.Instance);

        // Act
        var (GameId, NumberCodes, MaxMoves, FieldValues) = await gamesClient.StartGameAsync(GameType.Game6x4, "test", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(4, NumberCodes);
        Assert.Equal(12, MaxMoves);
        Assert.Single(FieldValues.Keys);
        Assert.Equal("colors", FieldValues.Keys.First());
        Assert.Equal(6, FieldValues["colors"].Length);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task StartGameAsync_Should_StartGameAndTriggerGameCreatedEvent()
    {
        // Arrange
        (var httpClient, var handlerMock) = GetHttpClientReturningACreatedGameSkeleton();
        bool startActivityReceived = false;
        bool stopActivityReceived = false;

        using ActivityListener listener = new()
        {
            ShouldListenTo = _ => true,
            ActivityStarted = activity =>
            {
                if (activity.OperationName == "StartGameAsync")
                {
                    startActivityReceived = true;
                    Assert.Equal(ActivityKind.Client, activity.Kind);
                }
            },
            ActivityStopped = activity =>
            {
                if (activity.OperationName == "StartGameAsync")
                {
                    stopActivityReceived = true;
                    string? gameId = activity.GetBaggageItem("gameId");
                    Assert.NotNull(gameId);  // gameId needs to be part of the baggage
                    ActivityEvent? gameCreatedEvent = activity.Events.FirstOrDefault(e => e.Name == "GameCreated");
                    Assert.NotNull(gameCreatedEvent);
                    var tag = gameCreatedEvent.Value.Tags.FirstOrDefault(t => t.Key == "gameType");
                    Assert.Equal(tag.Value, "Game6x4");
   
                    Assert.Equal(ActivityKind.Client, activity.Kind);                  
                }
            },
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };

        ActivitySource.AddActivityListener(listener);
        
        GamesClient gamesClient = new(httpClient, NullLogger<GamesClient>.Instance);

        // Act
        var (GameId, NumberCodes, MaxMoves, FieldValues) = await gamesClient.StartGameAsync(GameType.Game6x4, "test", TestContext.Current.CancellationToken);

        Assert.True(startActivityReceived);
        Assert.True(stopActivityReceived);
    }

    [Fact]
    public async Task GetGamesAsync_Should_ReturnResults()
    {
        // Arrange
        (var httpClient, var handlerMock) = GetHttpClientReturningGamesSkeleton();

        GamesClient gamesClient = new(httpClient, NullLogger<GamesClient>.Instance);

        // Act
        GamesQuery query = new(GameType.Game6x4, "test", new DateOnly(2024, 2, 14), Ended: false);
        var games = await gamesClient.GetGamesAsync(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, games.Count());
    }

    [Fact]
    public async Task SetMoveAsync_Should_ReturnResults()
    {
        // Arrange
        (var httpClient, var handlerMock) = GetHttpClientReturningSetMoveSkeleton();

        GamesClient gamesClient = new(httpClient, NullLogger<GamesClient>.Instance);

        // Act
        var gameId = Guid.Parse("91f3c729-5e6e-459a-b656-2d77f3f45dd9");
        var (Results, Ended, IsVictory) = await gamesClient.SetMoveAsync(
            gameId,
            "test",
            GameType.Game6x4,
            1,
            ["Red", "Green", "Blue", "Yellow"],
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, Results.Length);
        Assert.Equal("White", Results[0]);
        Assert.Equal("White", Results[1]);
        Assert.False(Ended);
        Assert.False(IsVictory);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetGameAsync_Should_ReturnResults()
    {
        // Arrange
        (var httpClient, var handlerMock) = GetHttpClientReturningGameSkeleton();

        GamesClient gamesClient = new(httpClient, NullLogger<GamesClient>.Instance);

        // Act
        var gameId = Guid.Parse("91f3c729-5e6e-459a-b656-2d77f3f45dd9");
        var game = await gamesClient.GetGameAsync(gameId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(game);
        Assert.Equal(gameId, game.Id);
        Assert.Equal("Game6x4", game.GameType.ToString());
        Assert.Equal("test", game.PlayerName);
        Assert.Equal(1, game.LastMoveNumber);
        Assert.Equal(4, game.NumberCodes);
        Assert.Equal(12, game.MaxMoves);
        Assert.False(game.IsVictory);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CancelGameAsync_Should_CallApi()
    {
        // Arrange
        (var httpClient, var handlerMock) = GetHttpClientForCancelGame();

        GamesClient gamesClient = new(httpClient, NullLogger<GamesClient>.Instance);

        // Act
        var gameId = Guid.Parse("91f3c729-5e6e-459a-b656-2d77f3f45dd9");
        await gamesClient.CancelGameAsync(gameId, "test", GameType.Game6x4, TestContext.Current.CancellationToken);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CancelGameAsync_Should_TrackActivity()
    {
        // Arrange
        (var httpClient, var handlerMock) = GetHttpClientForCancelGame();
        bool startActivityReceived = false;
        bool stopActivityReceived = false;

        using ActivityListener listener = new()
        {
            ShouldListenTo = _ => true,
            ActivityStarted = activity =>
            {
                if (activity.OperationName == "CancelGameAsync")
                {
                    startActivityReceived = true;
                    Assert.Equal(ActivityKind.Client, activity.Kind);
                }
            },
            ActivityStopped = activity =>
            {
                if (activity.OperationName == "CancelGameAsync")
                {
                    stopActivityReceived = true;
                    ActivityEvent? gameCanceledEvent = activity.Events.FirstOrDefault(e => e.Name == "GameCanceled");
                    
                    // Check if the event exists
                    if (gameCanceledEvent != null)
                    {
                        // Check if there are any tags
                        var tags = gameCanceledEvent.Value.Tags;
                        foreach (var tag in tags)
                        {
                            if (tag.Key == "gameId")
                            {
                                // We found the gameId tag
                                Assert.NotNull(tag.Value);
                            }
                        }
                    }
                    
                    Assert.Equal(ActivityKind.Client, activity.Kind);
                }
            },
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };

        ActivitySource.AddActivityListener(listener);

        GamesClient gamesClient = new(httpClient, NullLogger<GamesClient>.Instance);

        // Act
        var gameId = Guid.Parse("91f3c729-5e6e-459a-b656-2d77f3f45dd9");
        await gamesClient.CancelGameAsync(gameId, "test", GameType.Game6x4, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(startActivityReceived);
        Assert.True(stopActivityReceived);
    }

    private static (HttpClient Client, Mock<HttpMessageHandler> Handler) GetHttpClientReturningACreatedGameSkeleton()
    {
        Mock<IConfiguration> configMock = new();
        configMock.Setup(x => x[It.IsAny<string>()]).Returns("http://localhost:5000");

        Mock<HttpMessageHandler> handlerMock = new(MockBehavior.Strict);
        string returnMessage = """
        {
            "id": "af8dd39f-6e16-41ef-9155-dcd3cf081e87",
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
                Content = new StringContent(returnMessage, Encoding.UTF8, "application/json")
            }).Verifiable();

        return (new(handlerMock.Object)
        {
            BaseAddress = new Uri(configMock.Object["GameAPIs"] ?? throw new InvalidOperationException())
        }, handlerMock);
    }

    private static (HttpClient Client, Mock<HttpMessageHandler> Handler) GetHttpClientReturningGamesSkeleton()
    {
        Mock<IConfiguration> configMock = new();
        configMock.Setup(x => x[It.IsAny<string>()]).Returns("http://localhost:5000");

        Mock<HttpMessageHandler> handlerMock = new(MockBehavior.Strict);
        string returnMessage = """
        [
          {
            "id": "91f3c729-5e6e-459a-b656-2d77f3f45dd9",
            "gameType": "Game6x4",
            "playerName": "test",
            "playerIsAuthenticated": false,
            "startTime": "2024-02-14T18:14:49.4420411Z",
            "endTime": null,
            "duration": null,
            "lastMoveNumber": 1,
            "numberCodes": 4,
            "maxMoves": 12,
            "isVictory": false,
            "fieldValues": {
              "colors": [
                "Red",
                "Green",
                "Blue",
                "Yellow",
                "Purple",
                "Orange"
              ]
            },
            "codes": [
              "Yellow",
              "Yellow",
              "Green",
              "Green"
            ],
            "moves": [
              {
                "id": "963baba8-44b8-45e5-81f3-193959ae5bf6",
                "moveNumber": 1,
                "guessPegs": [
                  "Red",
                  "Green",
                  "Blue",
                  "Yellow"
                ],
                "keyPegs": [
                  "White",
                  "White"
                ]
              }
            ]
          },
          {
            "id": "bbab5508-945b-4122-958b-576b6a5088af",
            "gameType": "Game6x4",
            "playerName": "test",
            "playerIsAuthenticated": false,
            "startTime": "2024-02-14T17:57:18.8385818Z",
            "endTime": "2024-02-14T17:59:28.1685725Z",
            "duration": "00:02:09.3299907",
            "lastMoveNumber": 4,
            "numberCodes": 4,
            "maxMoves": 12,
            "isVictory": true,
            "fieldValues": {
              "colors": [
                "Red",
                "Green",
                "Blue",
                "Yellow",
                "Purple",
                "Orange"
              ]
            },
            "codes": [
              "Red",
              "Yellow",
              "Red",
              "Green"
            ],
            "moves": [
              {
                "id": "a51f3ff2-dac6-47e9-8593-a069706bd095",
                "moveNumber": 1,
                "guessPegs": [
                  "Red",
                  "Green",
                  "Blue",
                  "Yellow"
                ],
                "keyPegs": [
                  "Black",
                  "White",
                  "White"
                ]
              },
              {
                "id": "cc6bfe41-37ce-4bf0-ab81-1b1f2169e87d",
                "moveNumber": 2,
                "guessPegs": [
                  "Red",
                  "Blue",
                  "Green",
                  "Purple"
                ],
                "keyPegs": [
                  "Black",
                  "White"
                ]
              },
              {
                "id": "bcfe6f5c-7222-4716-b410-f445daecbee3",
                "moveNumber": 3,
                "guessPegs": [
                  "Red",
                  "Yellow",
                  "Orange",
                  "Blue"
                ],
                "keyPegs": [
                  "Black",
                  "Black"
                ]
              },
              {
                "id": "6e245df6-47a5-42ef-a459-3393ba2dd50a",
                "moveNumber": 4,
                "guessPegs": [
                  "Red",
                  "Yellow",
                  "Red",
                  "Green"
                ],
                "keyPegs": [
                  "Black",
                  "Black",
                  "Black",
                  "Black"
                ]
              }
            ]
          }
        ]
        """;
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(returnMessage, Encoding.UTF8, "application/json")
            }).Verifiable();

        return (new(handlerMock.Object)
        {
            BaseAddress = new Uri(configMock.Object["GameAPIs"] ?? throw new InvalidOperationException())
        }, handlerMock);
    }

    private static (HttpClient Client, Mock<HttpMessageHandler> Handler) GetHttpClientReturningSetMoveSkeleton()
    {
        Mock<IConfiguration> configMock = new();
        configMock.Setup(x => x[It.IsAny<string>()]).Returns("http://localhost:5000");

        Mock<HttpMessageHandler> handlerMock = new(MockBehavior.Strict);
        string returnMessage = """
        {
          "id": "91f3c729-5e6e-459a-b656-2d77f3f45dd9",
          "gameType": "Game6x4",
          "moveNumber": 1,
          "ended": false,
          "isVictory": false,
          "results": ["White", "White"]
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
                Content = new StringContent(returnMessage, Encoding.UTF8, "application/json")
            }).Verifiable();

        return (new(handlerMock.Object)
        {
            BaseAddress = new Uri(configMock.Object["GameAPIs"] ?? throw new InvalidOperationException())
        }, handlerMock);
    }

    private static (HttpClient Client, Mock<HttpMessageHandler> Handler) GetHttpClientReturningGameSkeleton()
    {
        Mock<IConfiguration> configMock = new();
        configMock.Setup(x => x[It.IsAny<string>()]).Returns("http://localhost:5000");

        Mock<HttpMessageHandler> handlerMock = new(MockBehavior.Strict);
        string returnMessage = """
        {
          "id": "91f3c729-5e6e-459a-b656-2d77f3f45dd9",
          "gameType": "Game6x4",
          "playerName": "test",
          "playerIsAuthenticated": false,
          "startTime": "2024-02-14T18:14:49.4420411Z",
          "endTime": null,
          "duration": null,
          "lastMoveNumber": 1,
          "numberCodes": 4,
          "maxMoves": 12,
          "isVictory": false,
          "fieldValues": {
            "colors": [
              "Red",
              "Green",
              "Blue",
              "Yellow",
              "Purple",
              "Orange"
            ]
          },
          "codes": [
            "Yellow",
            "Yellow",
            "Green",
            "Green"
          ],
          "moves": [
            {
              "id": "963baba8-44b8-45e5-81f3-193959ae5bf6",
              "moveNumber": 1,
              "guessPegs": [
                "Red",
                "Green",
                "Blue",
                "Yellow"
              ],
              "keyPegs": [
                "White",
                "White"
              ]
            }
          ]
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
                Content = new StringContent(returnMessage, Encoding.UTF8, "application/json")
            }).Verifiable();

        return (new(handlerMock.Object)
        {
            BaseAddress = new Uri(configMock.Object["GameAPIs"] ?? throw new InvalidOperationException())
        }, handlerMock);
    }

    private static (HttpClient Client, Mock<HttpMessageHandler> Handler) GetHttpClientForCancelGame()
    {
        Mock<IConfiguration> configMock = new();
        configMock.Setup(x => x[It.IsAny<string>()]).Returns("http://localhost:5000");

        Mock<HttpMessageHandler> handlerMock = new(MockBehavior.Strict);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            }).Verifiable();

        return (new(handlerMock.Object)
        {
            BaseAddress = new Uri(configMock.Object["GameAPIs"] ?? throw new InvalidOperationException())
        }, handlerMock);
    }
}
