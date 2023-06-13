using System.Collections;

using Codebreaker.GameAPIs.Analyzers;
using Codebreaker.GameAPIs.Models;

using static Codebreaker.GameAPIs.Models.Colors;

namespace Codebreaker.GameAPIs.Algorithms.Tests;

public class ColorGame8x5AlgorithmTests
{
    [Fact]
    public void SetMoveWithThreeWhite()
    {
        ColorResult expectedKeyPegs = new(0, 3);
        ColorResult? resultKeyPegs = TestSkeleton(
            new[] { Green, Yellow, Green, Black, Orange },
            new[] { Yellow, Green, Black, Blue, Blue }
        );

        Assert.Equal(expectedKeyPegs, resultKeyPegs);
    }

    [InlineData(1, 2, Red, Yellow, Red, Blue, Orange)]
    [InlineData(2, 0, White, White, Blue, Red, White)]
    [Theory]
    public void SetMoveUsingVariousData(int expectedBlack, int expectedWhite, params string[] guessValues)
    {
        string[] code = new[] { Red, Green, Blue, Red, Brown };
        ColorResult expectedKeyPegs = new (expectedBlack, expectedWhite);
        ColorResult resultKeyPegs = TestSkeleton(code, guessValues);
        Assert.Equal(expectedKeyPegs, resultKeyPegs);
    }

    [Theory]
    [ClassData(typeof(TestData8x5))]
    public void SetMoveUsingVariousDataUsingDataClass(string[] code, string[] guess, ColorResult expectedKeyPegs)
    {
        ColorResult actualKeyPegs = TestSkeleton(code, guess);
        Assert.Equal(expectedKeyPegs, actualKeyPegs);
    }

    [Fact]
    public void ShouldThrowOnInvalidGuessCount()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            TestSkeleton(
                new[] { "Black", "Black", "Black", "Black", "Black" },
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
                new[] { "Black", "Black", "Black", "Black", "Black" },
                new[] { "Black", "Der", "Blue", "Yellow", "Black" }      // "Der" is the wrong value
            );
        });
    }

    private static ColorResult TestSkeleton(string[] codes, string[] guesses)
    {
        MockColorGame game = new()
        {
            GameType = GameTypes.Game8x5,
            NumberPositions = 5,
            MaxMoves = 14,
            Won = false,
            FieldValues = new ColorField[] { Red, Blue, Green, Yellow, Black, White, Purple, Orange },
            Codes = codes.Select(c => new ColorField(c)).ToArray()
        };

        var guessPegs = guesses.Select(g => new ColorField(g)).ToList();
        ColorGameGuessAnalyzer analyzer = new(game, guessPegs, 1);
        return analyzer.GetResult();
    }
}

public class TestData8x5 : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new string[] { Green, Blue,  Green, Yellow, Orange }, // code
            new string[] { Green, Green, Black, White, Black },  // inputdata
            new ColorResult(1, 1) // expected
        };
        yield return new object[]
        {
            new string[] { Red, Blue, Black, White, Orange },
            new string[] { Black, Black, Red, Yellow, Yellow },
            new ColorResult(0, 2)
        };
        yield return new object[]
        {
            new string[] { Yellow, Black, Yellow, Green, Orange },
            new string[] { Black,  Black, Black,  Black, Black },
            new ColorResult(1, 0)
        };
        yield return new object[]
        {
            new string[] { Yellow, Yellow, White, Red, Orange },
            new string[] { Green,  Yellow, White, Red, Green },
            new ColorResult(3, 0)
        };
        yield return new object[]
        {
            new string[] { White, Black, Yellow, Black, Orange },
            new string[] { Black, Blue,  Black,  White, White },
            new ColorResult(0, 3)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
