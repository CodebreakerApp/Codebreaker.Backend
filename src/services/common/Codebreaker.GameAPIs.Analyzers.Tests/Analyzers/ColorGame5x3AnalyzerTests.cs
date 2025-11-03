using System.Collections;

using static Codebreaker.GameAPIs.Models.Colors;

namespace Codebreaker.GameAPIs.Analyzer.Tests;

public class ColorGame5x3AnalyzerTests
{
    [Fact]
    public void SetMove_ShouldReturnTwoWhite()
    {
        ColorResult expectedKeyPegs = new(0, 2);
        ColorResult? resultKeyPegs = AnalyzeGame(
            [Red, Green, Blue],
            [Green, Blue, Yellow]
        );

        Assert.Equal(expectedKeyPegs, resultKeyPegs);
    }

    [InlineData(2, 0, Red, Green, Red)]
    [InlineData(3, 0, Red, Green, Blue)]
    [Theory]
    public void SetMove_UsingVariousData(int expectedBlack, int expectedWhite, params string[] guessValues)
    {
        string[] code = [Red, Green, Blue];
        ColorResult expectedKeyPegs = new(expectedBlack, expectedWhite);
        ColorResult resultKeyPegs = AnalyzeGame(code, guessValues);
        Assert.Equal(expectedKeyPegs, resultKeyPegs);
    }

    [Theory]
    [ClassData(typeof(TestData5x3))]
    public void SetMove_UsingVariousDataUsingDataClass(string[] code, string[] guess, ColorResult expectedKeyPegs)
    {
        ColorResult actualKeyPegs = AnalyzeGame(code, guess);
        Assert.Equal(expectedKeyPegs, actualKeyPegs);
    }

    [Fact]
    public void SetMove_ShouldThrowOnInvalidGuessCount()
    {
        Assert.Throws<ArgumentException>(() =>
            AnalyzeGame(
                [Red, Green, Blue],
                [Red]
            ));
    }

    [Fact]
    public void SetMove_ShouldThrowOnInvalidGuessValues()
    {
        Assert.Throws<ArgumentException>(() =>
            AnalyzeGame(
                [Red, Green, Blue],
                [Red, "Invalid", Blue]
            ));
    }

    [Fact]
    public void SetMove_ShouldThrowOnInvalidMoveNumber()
    {
        Assert.Throws<ArgumentException>(() =>
            AnalyzeGame(
                [Red, Green, Blue],
                [Green, Red, Blue], moveNumber: 2));
    }

    [Fact]
    public void SetMove_ShouldNotIncrementMoveNumberOnInvalidMove()
    {
        IGame game = AnalyzeGameCatchingException(
            [Red, Green, Blue],
            [Green, Red, Blue], moveNumber: 2);

        Assert.Equal(0, game?.LastMoveNumber);
    }

    [Fact]
    public void SetMove_ShouldIncludeExpectedMoveNumberInErrorMessage()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            AnalyzeGame(
                [Red, Green, Blue],
                [Green, Red, Blue], moveNumber: 3));

        Assert.Contains("received 3", exception.Message);
        Assert.Contains("expected 1", exception.Message);
    }

    private static MockColorGame CreateGame(string[] codes) =>
        new()
        {
            GameType = GameTypes.Game5x3,
            NumberCodes = 3,
            MaxMoves = 10,
            IsVictory = false,
            FieldValues = new Dictionary<string, IEnumerable<string>>()
            {
                [FieldCategories.Colors] = [.. TestData5x3.Colors5]
            },
            Codes = codes
        };

    private static ColorResult AnalyzeGame(string[] codes, string[] guesses, int moveNumber = 1)
    {
        MockColorGame game = CreateGame(codes);
        ColorGameGuessAnalyzer analyzer = new(game, [.. guesses.ToPegs<ColorField>()], moveNumber);
        return analyzer.GetResult();
    }

    private static IGame AnalyzeGameCatchingException(string[] codes, string[] guesses, int moveNumber = 1)
    {
        MockColorGame game = CreateGame(codes);

        ColorGameGuessAnalyzer analyzer = new(game, guesses.ToPegs<ColorField>().ToArray(), moveNumber);
        try
        {
            analyzer.GetResult();
        }
        catch (ArgumentException)
        {

        }
        return game;
    }
}

public class TestData5x3 : IEnumerable<object[]>
{
    public static readonly string[] Colors5 = [Red, Green, Blue, Yellow, Purple];

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new string[] { Red, Green, Blue }, // code
            new string[] { Red, Red, Yellow },  // inputdata
            new ColorResult(1, 0) // expected
        };
        yield return new object[]
        {
            new string[] { Red, Green, Blue },
            new string[] { Green, Blue, Red },
            new ColorResult(0, 3)
        };
        yield return new object[]
        {
            new string[] { Yellow, Purple, Green },
            new string[] { Purple, Purple, Purple },
            new ColorResult(1, 0)
        };
        yield return new object[]
        {
            new string[] { Red, Green, Blue },
            new string[] { Red, Green, Blue },
            new ColorResult(3, 0)
        };
        yield return new object[]
        {
            new string[] { Yellow, Blue, Red },
            new string[] { Blue, Yellow, Yellow },
            new ColorResult(0, 2)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
