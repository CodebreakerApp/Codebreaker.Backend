using CodeBreaker.APIs.Data;
using CodeBreaker.APIs.Services;
using CodeBreaker.Shared;

using Microsoft.Extensions.Logging;

using System.Collections;

using Xunit;

namespace CodeBreaker.APIs.Tests;

internal class TestContext : ICodeBreakerContext
{
    public Task InitGameAsync(CodeBreakerGame game)
    {
        return Task.CompletedTask;
    }
    
    public Task<CodeBreakerGame?> GetGameAsync(string gameId)
    {
        return Task.FromResult<CodeBreakerGame?>(null);
    }
    
    public Task UpdateGameAsync(CodeBreakerGame game)
    {
        return Task.CompletedTask;
    }
    public Task AddMoveAsync(CodeBreakerGameMove move)
    {
        return Task.CompletedTask;
    }

    public Task<GamesInformationDetail> GetGamesDetailsAsync(DateTime date)
    {
        return Task.FromResult<GamesInformationDetail>(null!);
    }
    public Task<IEnumerable<GamesInfo>> GetGamesAsync(DateTime date)
    {
        return Task.FromResult<IEnumerable<GamesInfo>>(null!);
    }

    public Task<CodeBreakerGame?> GetGameDetailAsync(string gameId)
    {
        return Task.FromResult<CodeBreakerGame?>(null);
    }
}

//public class TestGameInitializer : IGameInitializer<string>
//{
//    public string[] GetColors() =>
//        new string[] { "red", "green", "blue", "red" };
//}

public class TestLogger<T> : ILogger<T>, IDisposable
{
    public IDisposable? BeginScope<TState>(TState state) where TState: notnull => 
        this;
    
    public void Dispose() { }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
    }
}


public class GameServiceTests
{
    private Game6x4Service _gameService;
    private string? _gameId;

    public GameServiceTests()
    {
        _gameService = new Game6x4Service(new TestRandomGame6x4RedGreenBlueRedGenerator(), new GameCache(new TestLogger<GameCache>()), new TestContext(), new TestLogger<Game6x4Service>());
    }

    [Fact]
    public async void SetMoveWithThreeWhite()
    {
        string[] expected = { "white", "white", "white" };
        string[] code = { "green", "yellow", "green", "black" };
        Game6x4Service gameService = new Game6x4Service(new TestRandomGame6x4Generator(code), new GameCache(new TestLogger<GameCache>()), new TestContext(), new TestLogger<Game6x4Service>());
        string gameId = await gameService.StartGameAsync("user", GameTypes.Game6x4);

        GameMove move = new(gameId, 1, new[] { "yellow", "green", "black", "blue" });
        var result = await gameService.SetMoveAsync(move);
        var actual = result.KeyPegs;
        Assert.Equal(expected, actual);
    }

    [InlineData("red", "yellow", "red", "blue", "black", "white", "white")]
    [InlineData("white", "white", "blue", "red", "black", "black")]
    [Theory]
    public async Task SetMoveAsync(string c1, string c2, string c3, string c4, params string[] expected)
    {
        _gameId = await _gameService.StartGameAsync("user", GameTypes.Game6x4);

        List<string> codePegs = new(new string[] { c1, c2, c3, c4 });

        GameMove move = new(_gameId, 1, codePegs);
        GameMoveResult gameResult = await _gameService.SetMoveAsync(move);
        string[] result = gameResult.KeyPegs.ToArray();
        Assert.Equal(expected, result);
    }

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task SetMoveAsync2(string[] code, string[] inputData, string[] results)
    {
        Game6x4Service gameService = new Game6x4Service(new TestRandomGame6x4Generator(code), new GameCache(new TestLogger<GameCache>()), new TestContext(), new TestLogger<Game6x4Service>());
        string gameId = await gameService.StartGameAsync("user", GameTypes.Game6x4);

        string[] expected = results;
        List<string> codePegs = new(inputData);

        GameMove move = new(gameId, 1, codePegs);
        GameMoveResult gameResult = await gameService.SetMoveAsync(move);
        string[] actual = gameResult.KeyPegs.ToArray();
        Assert.Equal(expected, actual);
    }
}

public record TestRandomGame6x4RedGreenBlueRedGenerator() : RandomGame6x4Generator
{
    public override string[] GetPegs() => new string[] { "red", "green", "blue", "red" };
}

public record TestRandomGame6x4Generator(string[] Codes) : RandomGame6x4Generator
{
    public override string[] GetPegs() => Codes;
}

public class TestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new string[] { "green", "blue", "green", "yellow" }, // code
            new string[] { "green", "green", "black", "white" }, // inputdata
            new string[] { "black", "white" } // results
        };
        yield return new object[]
        {
            new string[] { "red", "blue", "black", "white" }, // code
            new string[] { "black", "black", "red", "yellow" }, // inputdata
            new string[] { "white", "white" } // results
        };
        yield return new object[]
        {
            new string[] { "yellow", "black", "yellow", "green" },
            new string[] { "black", "black", "black", "black" },
            new string[] { "black" }
        };
        yield return new object[]
        {
            new string[] { "yellow", "yellow", "white", "red" },
            new string[] { "green", "yellow", "white", "red" },
            new string[] { "black", "black", "black" }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
