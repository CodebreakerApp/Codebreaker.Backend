using CodeBreaker.APIs.Factories.GameTypeFactories;
using CodeBreaker.APIs.Extensions;
using CodeBreaker.Shared.Models.Data;

using System.Collections;

using Xunit;

using static CodeBreaker.Shared.Models.Data.Colors;

namespace CodeBreaker.APIs.Tests;

public class GameAlgorithmTests
{
    [Fact]
    public void SetMoveWithThreeWhite()
    {
        KeyPegs expectedKeyPegs = new(0, 3);
        KeyPegs? resultKeyPegs = TestSkeleton(
            new[] { Green, Yellow, Green, Black },
            new[] { Yellow, Green, Black, Blue }
        );

        Assert.Equal(expectedKeyPegs, resultKeyPegs);
    }

    [InlineData(1, 2, Red, Yellow, Red, Blue)]
    [InlineData(2, 0, White, White, Blue, Red)]
    [Theory]
    public void SetMoveUsingVariousData(int expectedBlack, int expectedWhite, params string[] guessValues)
    {
        string[] code = new[] { Red, Green, Blue, Red };
        KeyPegs expectedKeyPegs = new KeyPegs(expectedBlack, expectedWhite);
        KeyPegs? resultKeyPegs = TestSkeleton(code, guessValues);
        Assert.Equal(expectedKeyPegs, resultKeyPegs);
    }

    [Theory]
    [ClassData(typeof(TestData))]
    public void SetMoveUsingVariousDataUsingDataClass(string[] code, string[] guess, KeyPegs expectedKeyPegs)
    {
        KeyPegs? actualKeyPegs = TestSkeleton(code, guess);
        Assert.Equal(expectedKeyPegs, actualKeyPegs);
    }

    [Fact]
    public void ShouldThrowOnInvalidGuessCount()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            TestSkeleton(
                new[] { "Black", "Black", "Black", "Black" },
                new[] { "Black" }
            );
        });
    }

    [Fact]
    public void ShouldThrowOnInvalidGuessValues()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            TestSkeleton(
                new[] { "Black", "Black", "Black", "Black" },
                new[] { "Black", "Der", "Blue", "Yellow" }      // "Der" is the wrong value
            );
        });
    }
    private KeyPegs? TestSkeleton(string[] code, string[] guess)
    {
        Game game = new(
            Guid.NewGuid(),
            new GameType6x4Factory().Create(),
            "test-username",
            code
        );
        Move move = new(1, guess);

        game.ApplyMove(move);

        return game.Moves[0].KeyPegs;
    }
}

public class TestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new string[] { Green, Blue,  Green, Yellow }, // code
            new string[] { Green, Green, Black, White },  // inputdata
            new KeyPegs(1, 1) // expected
        };
        yield return new object[]
        {
            new string[] { Red,   Blue,  Black, White },
            new string[] { Black, Black, Red,   Yellow },
            new KeyPegs(0, 2)
        };
        yield return new object[]
        {
            new string[] { Yellow, Black, Yellow, Green },
            new string[] { Black,  Black, Black,  Black },
            new KeyPegs(1, 0)
        };
        yield return new object[]
        {
            new string[] { Yellow, Yellow, White, Red },
            new string[] { Green,  Yellow, White, Red },
            new KeyPegs(3, 0)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
