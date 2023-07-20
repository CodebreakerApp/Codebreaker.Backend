using System.Collections;

using Codebreaker.GameAPIs.Algorithms.Fields;
using Codebreaker.GameAPIs.Analyzers;
using Codebreaker.GameAPIs.Algorithms.Extensions;
using Codebreaker.GameAPIs.Models;

using static Codebreaker.GameAPIs.Models.Colors;

namespace Codebreaker.GameAPIs.Algorithms.Tests;

public class ColorGame6x4AnalyzerTests
{
    [Fact]
    public void SetMoveWithThreeWhite()
    {
        ColorResult expectedKeyPegs = new(0, 3);
        ColorResult? resultKeyPegs = TestSkeleton(
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
        ColorResult expectedKeyPegs = new (expectedBlack, expectedWhite);
        ColorResult resultKeyPegs = TestSkeleton(code, guessValues);
        Assert.Equal(expectedKeyPegs, resultKeyPegs);
    }

    [Theory]
    [ClassData(typeof(TestData6x4))]
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

    private static ColorResult TestSkeleton(string[] codes, string[] guesses)
    {
        MockColorGame game = new()
        {
            GameType = GameTypes.Game6x4,
            NumberCodes = 4,
            MaxMoves = 12,
            IsVictory = false,
            FieldValues = new Dictionary<string, IEnumerable<string>>()
            {
                [FieldCategories.Colors ] = TestData6x4.Colors6.ToList()
            },
            Codes = codes
        };

        ColorGameGuessAnalyzer analyzer = new(game,guesses.ToPegs<ColorField>().ToArray(), 1);
        return analyzer.GetResult();
    }
}

public class TestData6x4 : IEnumerable<object[]>
{
    public static readonly string[] Colors6 = { Red, Green, Blue, Yellow, Black, White };

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new string[] { Green, Blue,  Green, Yellow }, // code
            new string[] { Green, Green, Black, White },  // inputdata
            new ColorResult(1, 1) // expected
        };
        yield return new object[]
        {
            new string[] { Red,   Blue,  Black, White },
            new string[] { Black, Black, Red,   Yellow },
            new ColorResult(0, 2)
        };
        yield return new object[]
        {
            new string[] { Yellow, Black, Yellow, Green },
            new string[] { Black,  Black, Black,  Black },
            new ColorResult(1, 0)
        };
        yield return new object[]
        {
            new string[] { Yellow, Yellow, White, Red },
            new string[] { Green,  Yellow, White, Red },
            new ColorResult(3, 0)
        };
        yield return new object[]
        {
            new string[] { White, Black, Yellow, Black },
            new string[] { Black, Blue,  Black,  White },
            new ColorResult(0, 3)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
