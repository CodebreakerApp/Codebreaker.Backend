using System.Collections;

using Codebreaker.GameAPIs.Extensions;
using Codebreaker.GameAPIs.Models;

using static Codebreaker.GameAPIs.Models.Colors;

namespace Codebreaker.GameAPIs.Algorithms.Tests;

public class ColorGameAlgorithmTests
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
    [ClassData(typeof(TestData))]
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

    private ColorResult TestSkeleton(string[] codes, string[] guesses)
    {
        MockColorGame game = new()
        {
            Holes = 4,
            MaxMoves = 12,
            Won = false,
            Fields = new List<ColorField>() { Red, Blue, Green, Yellow, Black, White },
            Codes = new List<ColorField>(codes.Select(c => new ColorField(c)))
        };

        MockColorMove move = new()
        {
            MoveNumber = 1,
            GuessPegs = new List<ColorField>(guesses.Select(g => new ColorField(g)))
        };
        
        game.ApplyMove(move);

        return game.Moves.First().KeyPegs;
    }
}

public class MockColorGame : ICalculatableGame<ColorField, ColorResult>
{
    public int Holes { get; init; }
    public int MaxMoves { get; init; }
    public DateTime EndTime { get; set; }
    public bool Won { get; set; }

    private List<ColorField> _fields = new();
    public required IEnumerable<ColorField> Fields { get; init; }
    public required ICollection<ColorField> Codes { get; init; }

    private List<ICalculatableMove<ColorField, ColorResult>> _moves = new();
    public ICollection<ICalculatableMove<ColorField, ColorResult>> Moves => _moves;
}

public class MockColorMove : ICalculatableMove<ColorField, ColorResult>
{
    public int MoveNumber { get; set; }
    public required ICollection<ColorField> GuessPegs { get; init; }
    public ColorResult KeyPegs { get; set; }
}

public class TestData : IEnumerable<object[]>
{
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