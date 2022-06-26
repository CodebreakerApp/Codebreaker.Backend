using CodeBreaker.APIs.Data;
using CodeBreaker.APIs.Services;
using CodeBreaker.Shared;

using Microsoft.Extensions.Logging;

using System.Collections;

using Xunit;

using static CodeBreaker.Shared.CodeBreakerColors;

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

// TODO: change GameServiceTests to tests for game service and game algorithm

//public class GameServiceTests
//{
//    private Game6x4Service _gameService;
//    private string? _gameId;

//    public GameServiceTests()
//    {
//        _gameService = new Game6x4Service(new TestRandomGame6x4RedGreenBlueRedGenerator(), new GameCache(new TestLogger<GameCache>()), new TestContext(), new TestLogger<Game6x4Service>());
//    }

//    [Fact]
//    public async void SetMoveWithThreeWhite()
//    {
//        string[] expected = { White, White, White };
//        string[] code = { Green, Yellow, Green, Black };
//        Game6x4Service gameService = new Game6x4Service(new TestRandomGame6x4Generator(code), new GameCache(new TestLogger<GameCache>()), new TestContext(), new TestLogger<Game6x4Service>());
//        string gameId = await gameService.StartGameAsync("user", GameTypes.Game6x4);

//        GameMove move = new(gameId, 1, new[] { Yellow, Green, Black, Blue });
//        var result = await gameService.SetMoveAsync(move);
//        var actual = result.KeyPegs;
//        Assert.Equal(expected, actual);
//    }

//    [InlineData(Red, Yellow, Red, Blue, Black, White, White)]
//    [InlineData(White, White, Blue, Red, Black, Black)]
//    [Theory]
//    public async Task SetMoveAsync(string c1, string c2, string c3, string c4, params string[] expected)
//    {
//        _gameId = await _gameService.StartGameAsync("user", GameTypes.Game6x4);

//        List<string> codePegs = new(new string[] { c1, c2, c3, c4 });

//        GameMove move = new(_gameId, 1, codePegs);
//        GameMoveResult gameResult = await _gameService.SetMoveAsync(move);
//        string[] result = gameResult.KeyPegs.ToArray();
//        Assert.Equal(expected, result);
//    }

//    [Theory]
//    [ClassData(typeof(TestData))]
//    public async Task SetMoveAsync2(string[] code, string[] inputData, string[] results)
//    {
//        Game6x4Service gameService = new Game6x4Service(new TestRandomGame6x4Generator(code), new GameCache(new TestLogger<GameCache>()), new TestContext(), new TestLogger<Game6x4Service>());
//        string gameId = await gameService.StartGameAsync("user", GameTypes.Game6x4);

//        string[] expected = results;
//        List<string> codePegs = new(inputData);

//        GameMove move = new(gameId, 1, codePegs);
//        GameMoveResult gameResult = await gameService.SetMoveAsync(move);
//        string[] actual = gameResult.KeyPegs.ToArray();
//        Assert.Equal(expected, actual);
//    }
//}

public record TestRandomGame6x4RedGreenBlueRedGenerator() : RandomGame6x4
{
    public override string[] GetCode() => new string[] { Red, Green, Blue, Red };
}

public record TestRandomGame6x4Generator(string[] Codes) : RandomGame6x4
{
    public override string[] GetCode() => Codes;
}

public class TestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new string[] { Green, Blue, Green, Yellow }, // code
            new string[] { Green, Green, Black, White }, // inputdata
            new string[] { Black, White } // results
        };
        yield return new object[]
        {
            new string[] { Red, Blue, Black, White }, // code
            new string[] { Black, Black, Red, Yellow }, // inputdata
            new string[] { White, White } // results
        };
        yield return new object[]
        {
            new string[] { Yellow, Black, Yellow, Green },
            new string[] { Black, Black, Black, Black },
            new string[] { Black }
        };
        yield return new object[]
        {
            new string[] { Yellow, Yellow, White, Red },
            new string[] { Green, Yellow, White, Red },
            new string[] { Black, Black, Black }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
