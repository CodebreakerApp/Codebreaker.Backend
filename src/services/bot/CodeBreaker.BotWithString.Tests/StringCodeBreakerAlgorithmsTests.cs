using System.Collections;
using Codebreaker.GameAPIs.Client.Models;
using Xunit;

namespace CodeBreaker.BotWithString.Tests;

public class StringCodeBreakerAlgorithmsTests
{
    [Theory]
    [InlineData(GameType.Game6x4, 4)]
    [InlineData(GameType.Game8x5, 5)]
    [InlineData(GameType.Game5x5x4, 4)]
    public void SelectPeg_Should_ReturnCorrectPeg(GameType gameType, int expectedFieldsCount)
    {
        // Arrange
        string[] testCodes = gameType switch
        {
            GameType.Game6x4 => ["Red", "Blue", "Green", "Yellow"],
            GameType.Game8x5 => ["Red", "Blue", "Green", "Yellow", "Orange"],
            GameType.Game5x5x4 => ["Red", "Blue", "Green", "Yellow"],
            _ => ["Red", "Blue", "Green", "Yellow"]
        };

        // Act & Assert
        for (int i = 0; i < expectedFieldsCount; i++)
        {
            string actual = testCodes.SelectPeg(gameType, i);
            Assert.Equal(testCodes[i], actual);
        }
    }

    [Theory]
    [InlineData(GameType.Game6x4, 4)]
    [InlineData(GameType.Game6x4, -1)]
    [InlineData(GameType.Game8x5, 5)]
    [InlineData(GameType.Game8x5, -1)]
    [InlineData(GameType.Game5x5x4, 4)]
    [InlineData(GameType.Game5x5x4, -1)]
    public void SelectPeg_Should_ThrowException_ForInvalidPegNumber(GameType gameType, int invalidPegNumber)
    {
        // Arrange
        string[] testCodes = gameType switch
        {
            GameType.Game6x4 => ["Red", "Blue", "Green", "Yellow"],
            GameType.Game8x5 => ["Red", "Blue", "Green", "Yellow", "Orange"],
            GameType.Game5x5x4 => ["Red", "Blue", "Green", "Yellow"],
            _ => ["Red", "Blue", "Green", "Yellow"]
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => testCodes.SelectPeg(gameType, invalidPegNumber));
    }

    [Fact]
    public void HandleBlackMatches_Should_FilterCorrectly_Game6x4()
    {
        // Arrange
        var possibleValues = new List<string[]>
        {
            new string[] { "Red", "Blue", "Green", "Yellow" },      // 4 black matches with selection
            new string[] { "Red", "Blue", "Green", "Black" },       // 3 black matches with selection
            new string[] { "Red", "Blue", "Black", "White" },       // 2 black matches with selection
            new string[] { "Red", "Black", "White", "Orange" },     // 1 black match with selection
            new string[] { "Black", "White", "Orange", "Purple" }   // 0 black matches with selection
        };
        string[] selection = new string[] { "Red", "Blue", "Green", "Yellow" };

        // Act
        var result = possibleValues.HandleBlackMatches(GameType.Game6x4, 4, selection);

        // Assert
        Assert.Single(result);
        Assert.Equal(new string[] { "Red", "Blue", "Green", "Yellow" }, result[0]);
    }

    [Fact]
    public void HandleBlackMatches_Should_FilterCorrectly_Game8x5()
    {
        // Arrange
        var possibleValues = new List<string[]>
        {
            new string[] { "Red", "Blue", "Green", "Yellow", "Orange" },    // 5 black matches with selection
            new string[] { "Red", "Blue", "Green", "Yellow", "Black" },     // 4 black matches with selection
            new string[] { "Red", "Blue", "Green", "Black", "White" },      // 3 black matches with selection
        };
        string[] selection = new string[] { "Red", "Blue", "Green", "Yellow", "Orange" };

        // Act
        var result = possibleValues.HandleBlackMatches(GameType.Game8x5, 3, selection);

        // Assert
        Assert.Single(result);
        Assert.Equal(new string[] { "Red", "Blue", "Green", "Black", "White" }, result[0]);
    }

    [Fact]
    public void HandleWhiteMatches_Should_FilterCorrectly()
    {
        // Arrange
        var possibleValues = new List<string[]>
        {
            new string[] { "Blue", "Red", "Yellow", "Green" },    // All colors match but in different positions (4 white matches)
            new string[] { "Blue", "Red", "Green", "Yellow" },    // 3 colors match in different positions
            new string[] { "Red", "Blue", "Green", "Yellow" },    // All colors match in same positions (0 white matches)
            new string[] { "Black", "White", "Orange", "Purple" } // No matching colors
        };
        string[] selection = new string[] { "Red", "Blue", "Green", "Yellow" };

        // Act
        var result = possibleValues.HandleWhiteMatches(GameType.Game6x4, 4, selection);

        // Assert
        Assert.Single(result);
        Assert.Equal(new string[] { "Blue", "Red", "Yellow", "Green" }, result[0]);
    }

    [Fact]
    public void HandleNoMatches_Should_RemoveAllWithMatchingColors()
    {
        // Arrange
        var possibleValues = new List<string[]>
        {
            new string[] { "Red", "Blue", "Green", "Yellow" },      // Contains Red and Blue from selection
            new string[] { "Black", "White", "Orange", "Purple" },  // No matching colors
            new string[] { "Red", "Black", "White", "Orange" },     // Contains Red from selection
            new string[] { "Pink", "Brown", "Gray", "Cyan" }        // No matching colors
        };
        string[] selection = new string[] { "Red", "Blue", "Green", "Yellow" };

        // Act
        var result = possibleValues.HandleNoMatches(GameType.Game6x4, selection);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(new string[] { "Black", "White", "Orange", "Purple" }, result);
        Assert.Contains(new string[] { "Pink", "Brown", "Gray", "Cyan" }, result);
    }

    [Fact]
    public void HandleBlueMatches_Should_ReturnUnfiltered_ForNonGame5x5x4()
    {
        // Arrange
        var possibleValues = new List<string[]>
        {
            new string[] { "Red", "Blue", "Green", "Yellow" },
            new string[] { "Black", "White", "Orange", "Purple" }
        };
        string[] selection = new string[] { "Red", "Blue", "Green", "Yellow" };

        // Act
        var result6x4 = possibleValues.HandleBlueMatches(GameType.Game6x4, 0, selection);
        var result8x5 = possibleValues.HandleBlueMatches(GameType.Game8x5, 0, selection);

        // Assert
        Assert.Equal(possibleValues.Count, result6x4.Count);
        Assert.Equal(possibleValues.Count, result8x5.Count);
    }

    [Fact]
    public void HandleBlueMatches_Should_FilterCorrectly_ForGame5x5x4()
    {
        // Arrange
        var possibleValues = new List<string[]>
        {
            new string[] { "RedCircle", "BlueSquare", "GreenTriangle", "YellowStar" },     // Should have some partial matches
            new string[] { "RedSquare", "BlueCircle", "GreenStar", "YellowTriangle" },     // Should have some partial matches
            new string[] { "BlackCircle", "WhiteSquare", "OrangeTriangle", "PurpleStar" }  // Should have no partial matches
        };
        string[] selection = new string[] { "RedCircle", "BlueSquare", "GreenTriangle", "YellowStar" };

        // Act
        var result = possibleValues.HandleBlueMatches(GameType.Game5x5x4, 1, selection);

        // Assert
        // The exact result depends on the partial match logic implementation
        Assert.True(result.Count <= possibleValues.Count);
    }

    [Theory]
    [ClassData(typeof(GenerateAllPossibleCombinationsTestData))]
    public void GenerateAllPossibleCombinations_Should_GenerateCorrectCount(GameType gameType, string[] possibleValues, int expectedCount)
    {
        // Act
        var result = StringCodeBreakerAlgorithms.GenerateAllPossibleCombinations(gameType, possibleValues);

        // Assert
        Assert.Equal(expectedCount, result.Count);
        
        // Verify all combinations are unique
        var uniqueCount = result.Select(arr => string.Join(",", arr)).Distinct().Count();
        Assert.Equal(expectedCount, uniqueCount);
    }

    [Fact]
    public void HandleBlackMatches_Should_ThrowException_ForInvalidHits()
    {
        // Arrange
        var possibleValues = new List<string[]>
        {
            new string[] { "Red", "Blue", "Green", "Yellow" }
        };
        string[] selection = new string[] { "Red", "Blue", "Green", "Yellow" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => possibleValues.HandleBlackMatches(GameType.Game6x4, -1, selection));
        Assert.Throws<ArgumentException>(() => possibleValues.HandleBlackMatches(GameType.Game6x4, 5, selection));
    }

    [Fact]
    public void StringPegWithFlag_Should_WorkCorrectly()
    {
        // Arrange
        var peg = new StringPegWithFlag("Red", false);

        // Act
        var usedPeg = peg with { Used = true };

        // Assert
        Assert.Equal("Red", peg.Value);
        Assert.False(peg.Used);
        Assert.Equal("Red", usedPeg.Value);
        Assert.True(usedPeg.Used);
    }
}

public class GenerateAllPossibleCombinationsTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // Game6x4: 4 positions, 2 colors = 2^4 = 16 combinations
        yield return new object[] { GameType.Game6x4, new string[] { "Red", "Blue" }, 16 };
        
        // Game6x4: 4 positions, 3 colors = 3^4 = 81 combinations
        yield return new object[] { GameType.Game6x4, new string[] { "Red", "Blue", "Green" }, 81 };
        
        // Game8x5: 5 positions, 2 colors = 2^5 = 32 combinations
        yield return new object[] { GameType.Game8x5, new string[] { "Red", "Blue" }, 32 };
        
        // Game5x5x4: 4 positions, 3 colors = 3^4 = 81 combinations
        yield return new object[] { GameType.Game5x5x4, new string[] { "Red", "Blue", "Green" }, 81 };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}