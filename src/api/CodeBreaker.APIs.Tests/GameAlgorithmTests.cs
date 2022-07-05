using CodeBreaker.APIs.Data;
using CodeBreaker.APIs.Services;
using CodeBreaker.Shared;

using System.Collections;

using Xunit;

using static CodeBreaker.Shared.CodeBreakerColors;

namespace CodeBreaker.APIs.Tests;

public class GameAlgorithmTests
{
    [Fact]
    public void SetMoveWithThreeWhite()
    {
        string[] expected = { White, White, White };
        string[] code = { Green, Yellow, Green, Black };
        string gameId = Guid.NewGuid().ToString();
        Game6x4Definition definition = new();

        Game game = new(gameId, "Game6x4", "test", code, definition.Colors, definition.Holes, definition.MaxMoves, DateTime.Now);
        GameAlgorithm algorithm = new GameAlgorithm(new TestLogger<GameAlgorithm>());

        
        GameMove guess = new(gameId, 1, new[] { Yellow, Green, Black, Blue });

        var result = algorithm.SetMove(game, guess);
        var actual = result.Result.KeyPegs;
        Assert.Equal(expected, actual);
    }

    [InlineData(Red, Yellow, Red, Blue, Black, White, White)]
    [InlineData(White, White, Blue, Red, Black, Black)]
    [Theory]
    public void SetMoveUsingVariousData(string guess1, string guess2, string guess3, string guess4, params string[] expected)
    {
        string gameId = Guid.NewGuid().ToString();
        string[] code = new[] { Red, Green, Blue, Red };
        Game6x4Definition definition = new();
        Game game = new(gameId, "Game6x4", "test", code, definition.Colors, definition.Holes, definition.MaxMoves, DateTime.Now);


        List<string> codePegs = new(new string[] { guess1, guess2, guess3, guess4 });
        GameAlgorithm algorithm = new(new TestLogger<GameAlgorithm>());

        GameMove guess = new(gameId, 1, codePegs);
        (GameMoveResult gameResult, _, _) = algorithm.SetMove(game, guess);
        string[] result = gameResult.KeyPegs.ToArray();
        Assert.Equal(expected, result);
    }

    [Theory]
    [ClassData(typeof(TestData))]
    public void SetMoveUsingVariousData2(string[] code, string[] guesses, string[] expected)
    {
        string gameId = Guid.NewGuid().ToString();
        Game6x4Definition definition = new();
        Game game = new(gameId, "Game6x4", "test", code, definition.Colors, definition.Holes, definition.MaxMoves, DateTime.Now);
        
        GameMove guess = new(gameId, 1, guesses);

        GameAlgorithm algorithm = new(new TestLogger<GameAlgorithm>());

        var result = algorithm.SetMove(game, guess);
        string[] actual = result.Result.KeyPegs.ToArray();
        Assert.Equal(expected, actual);
    }
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
