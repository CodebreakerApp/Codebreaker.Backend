using System.Collections;

using static Codebreaker.GameAPIs.Models.Colors;

namespace Codebreaker.GameAPIs.Analyzer.Tests;

public class SimpleGameGuessAnalyzerTests
{
    [Fact]
    public void SetMoveShouldReturnThreeCorrectColor()
    {
        SimpleColorResult expectedKeyPegs = new(
        [
            ResultValue.CorrectColor,
            ResultValue.CorrectColor,
            ResultValue.CorrectColor,
            ResultValue.Incorrect
        ]);
        SimpleColorResult? resultKeyPegs = TestSkeleton(
            [Green, Yellow, Green, Black],
            [Yellow, Green, Black, Blue]
        );

        Assert.Equal(expectedKeyPegs, resultKeyPegs);
    }

    [Theory]
    [ClassData(typeof(TestData6x4Mini))]
    public void SetMoveUsingVariousDataUsingDataClass(string[] code, string[] guess, SimpleColorResult expectedKeyPegs)
    {
        SimpleColorResult actualKeyPegs = TestSkeleton(code, guess);
        Assert.Equal(expectedKeyPegs, actualKeyPegs);
    }

    [Fact]
    public void ShouldThrowOnInvalidGuessCount()
    {
        Assert.Throws<ArgumentException>(() => 
            TestSkeleton(
                ["Black", "Black", "Black", "Black"],
                ["Black"]
            ));
    }

    [Fact]
    public void ShouldThrowOnInvalidGuessValues()
    {
        Assert.Throws<ArgumentException>(() => 
            TestSkeleton(
                ["Black", "Black", "Black", "Black"],
                ["Black", "Der", "Blue", "Yellow"]      // "Der" is the wrong value
            ));
    }

    [Fact]
    public void ShouldThrowOnInvalidMoveNumber()
    {
        Assert.Throws<ArgumentException>(() => 
            TestSkeleton(
                [Green, Yellow, Green, Black],
                [Yellow, Green, Black, Blue], moveNumber: 2));
    }

    [Fact]
    public void ShouldNotIncrementMoveNumberOnInvalidMove()
    {
        IGame game = TestSkeletonWithGame(
            [Green, Yellow, Green, Black],
            [Yellow, Green, Black, Blue], moveNumber: 2);

        Assert.Equal(0, game?.LastMoveNumber);
    }

    private static SimpleColorResult TestSkeleton(string[] codes, string[] guesses, int moveNumber = 1)
    {
        MockColorGame game = new()
        {
            GameType = GameTypes.Game6x4Mini,
            NumberCodes = 4,
            MaxMoves = 12,
            IsVictory = false,
            FieldValues = new Dictionary<string, IEnumerable<string>>()
            {
                [FieldCategories.Colors ] = TestData6x4.Colors6.ToList()
            },
            Codes = codes
        };

        SimpleGameGuessAnalyzer analyzer = new(game,guesses.ToPegs<ColorField>().ToArray(), moveNumber);
        return analyzer.GetResult();
    }

    private static IGame TestSkeletonWithGame(string[] codes, string[] guesses, int moveNumber = 1)
    {
        MockColorGame game = new()
        {
            GameType = GameTypes.Game6x4,
            NumberCodes = 4,
            MaxMoves = 12,
            IsVictory = false,
            FieldValues = new Dictionary<string, IEnumerable<string>>()
            {
                [FieldCategories.Colors] = TestData6x4.Colors6.ToList()
            },
            Codes = codes
        };

        SimpleGameGuessAnalyzer analyzer = new(game, guesses.ToPegs<ColorField>().ToArray(), moveNumber);
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

public class TestData6x4Mini : IEnumerable<object[]>
{
    public static readonly string[] Colors6 = [Red, Green, Blue, Yellow, Black, White];

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new string[] { Green, Blue,  Green, Yellow }, // code
            new string[] { Green, Green, Black, White },  // inputdata
            new SimpleColorResult(
            [
                ResultValue.CorrectPositionAndColor,
                ResultValue.CorrectColor, 
                ResultValue.Incorrect, 
                ResultValue.Incorrect
            ]) // expected
        };
        yield return new object[]
        {
            new string[] { Red,   Blue,  Black, White },
            new string[] { Black, Black, Red,   Yellow },
            new SimpleColorResult(
            [
                ResultValue.CorrectColor,
                ResultValue.Incorrect,
                ResultValue.CorrectColor,
                ResultValue.Incorrect
            ])
        };
        yield return new object[]
        {
            new string[] { Yellow, Black, Yellow, Green },
            new string[] { Black,  Black, Black,  Black },
            new SimpleColorResult(
            [
                ResultValue.Incorrect,
                ResultValue.CorrectPositionAndColor,
                ResultValue.Incorrect,
                ResultValue.Incorrect
            ])
        };
        yield return new object[]
        {
            new string[] { Yellow, Yellow, White, Red },
            new string[] { Green,  Yellow, White, Red },
            new SimpleColorResult(
            [
                ResultValue.Incorrect,
                ResultValue.CorrectPositionAndColor,
                ResultValue.CorrectPositionAndColor,
                ResultValue.CorrectPositionAndColor
            ])
        };
        yield return new object[]
        {
            new string[] { White, Black, Yellow, Black },
            new string[] { Black, Blue,  Black,  White },
            new SimpleColorResult(
                [
                    ResultValue.CorrectColor,
                    ResultValue.Incorrect,
                    ResultValue.CorrectColor,
                    ResultValue.CorrectColor
                ])
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
