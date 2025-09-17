using Codebreaker.GameAPIs.Client;
using Codebreaker.GameAPIs.Client.Models;
using Moq;

namespace CodeBreaker.BotWithString.Tests;

public class StringBotGameRunnerTests
{
    [Fact]
    public async Task PlayGameAsync_Should_StartGameAndMakeMove()
    {
        // Arrange
        var mockClient = new Mock<IGamesClient>();
        var gameId = Guid.NewGuid();
        var fieldValues = new Dictionary<string, string[]>
        {
            ["Colors"] = new[] { "Red", "Blue", "Green", "Yellow" }
        };

        mockClient.Setup(x => x.StartGameAsync(GameType.Game6x4, "TestPlayer", It.IsAny<CancellationToken>()))
            .ReturnsAsync((gameId, 4, 12, fieldValues));

        mockClient.Setup(x => x.SetMoveAsync(gameId, "TestPlayer", GameType.Game6x4, 1, 
            It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { "4", "0" }, true, true)); // 4 black hits = win

        var runner = new StringBotGameRunner(mockClient.Object);

        // Act
        var result = await runner.PlayGameAsync(GameType.Game6x4, "TestPlayer");

        // Assert
        Assert.Equal(gameId, result.GameId);
        Assert.Equal(GameType.Game6x4, result.GameType);
        Assert.Equal("TestPlayer", result.PlayerName);
        Assert.True(result.GameWon);
        Assert.True(result.GameEnded);
        Assert.Equal(1, result.MovesUsed);
        Assert.NotNull(result.WinningCombination);
        Assert.Equal(4, result.WinningCombination.Length);
    }

    [Fact]
    public async Task PlayGameAsync_Should_HandleMultipleMoves()
    {
        // Arrange
        var mockClient = new Mock<IGamesClient>();
        var gameId = Guid.NewGuid();
        var fieldValues = new Dictionary<string, string[]>
        {
            ["Colors"] = new[] { "Red", "Blue" }
        };

        mockClient.Setup(x => x.StartGameAsync(GameType.Game6x4, "TestPlayer", It.IsAny<CancellationToken>()))
            .ReturnsAsync((gameId, 4, 12, fieldValues));

        // First move: no matches
        mockClient.Setup(x => x.SetMoveAsync(gameId, "TestPlayer", GameType.Game6x4, 1,
            It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { "0", "0" }, false, false));

        // Second move: win
        mockClient.Setup(x => x.SetMoveAsync(gameId, "TestPlayer", GameType.Game6x4, 2,
            It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { "4", "0" }, true, true));

        var runner = new StringBotGameRunner(mockClient.Object);

        // Act
        var result = await runner.PlayGameAsync(GameType.Game6x4, "TestPlayer");

        // Assert
        Assert.Equal(gameId, result.GameId);
        Assert.True(result.GameWon);
        Assert.True(result.GameEnded);
        Assert.Equal(2, result.MovesUsed);
        
        // Verify both moves were called
        mockClient.Verify(x => x.SetMoveAsync(gameId, "TestPlayer", GameType.Game6x4, 1,
            It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
        mockClient.Verify(x => x.SetMoveAsync(gameId, "TestPlayer", GameType.Game6x4, 2,
            It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlayGameAsync_Should_HandleGameLoss()
    {
        // Arrange
        var mockClient = new Mock<IGamesClient>();
        var gameId = Guid.NewGuid();
        var fieldValues = new Dictionary<string, string[]>
        {
            ["Colors"] = new[] { "Red", "Blue" }
        };

        mockClient.Setup(x => x.StartGameAsync(GameType.Game6x4, "TestPlayer", It.IsAny<CancellationToken>()))
            .ReturnsAsync((gameId, 4, 2, fieldValues)); // Only 2 max moves

        // Both moves: no matches, game ends after max moves
        mockClient.Setup(x => x.SetMoveAsync(gameId, "TestPlayer", GameType.Game6x4, It.IsAny<int>(),
            It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { "0", "0" }, false, false));

        var runner = new StringBotGameRunner(mockClient.Object);

        // Act
        var result = await runner.PlayGameAsync(GameType.Game6x4, "TestPlayer");

        // Assert
        Assert.Equal(gameId, result.GameId);
        Assert.False(result.GameWon);
        Assert.False(result.GameEnded); // Game didn't officially end, just reached max moves
        Assert.Equal(2, result.MovesUsed);
        Assert.Null(result.WinningCombination);
    }

    [Fact]
    public async Task PlayGameAsync_Should_HandleGame8x5()
    {
        // Arrange
        var mockClient = new Mock<IGamesClient>();
        var gameId = Guid.NewGuid();
        var fieldValues = new Dictionary<string, string[]>
        {
            ["Colors"] = new[] { "Red", "Blue", "Green", "Yellow", "Orange" }
        };

        mockClient.Setup(x => x.StartGameAsync(GameType.Game8x5, "TestPlayer", It.IsAny<CancellationToken>()))
            .ReturnsAsync((gameId, 5, 14, fieldValues));

        mockClient.Setup(x => x.SetMoveAsync(gameId, "TestPlayer", GameType.Game8x5, 1,
            It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { "5", "0" }, true, true)); // 5 black hits = win

        var runner = new StringBotGameRunner(mockClient.Object);

        // Act
        var result = await runner.PlayGameAsync(GameType.Game8x5, "TestPlayer");

        // Assert
        Assert.Equal(GameType.Game8x5, result.GameType);
        Assert.True(result.GameWon);
        Assert.NotNull(result.WinningCombination);
        Assert.Equal(5, result.WinningCombination.Length);
    }
}