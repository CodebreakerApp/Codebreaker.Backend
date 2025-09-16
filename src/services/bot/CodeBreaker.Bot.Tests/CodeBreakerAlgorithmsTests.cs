using System.Collections;
using Codebreaker.GameAPIs.Client.Models;

using Xunit;

namespace CodeBreaker.Bot.Tests;

public class CodeBreakerAlgorithmsTests
{
    [Fact]
    public void SelectPeg_Should_ThrowException_ForGame6x4()
    {
        Assert.Throws<InvalidOperationException>(() => 
            CodeBreakerAlgorithms.SelectPeg(44, GameType.Game6x4, 4));
    }

    [Fact]
    public void SelectPeg_Should_ThrowException_ForGame8x5()
    {
        Assert.Throws<InvalidOperationException>(() => 
            CodeBreakerAlgorithms.SelectPeg(44, GameType.Game8x5, 5));
    }

    [Fact]
    public void SelectPeg_Should_ThrowException_ForGame5x5x4()
    {
        Assert.Throws<InvalidOperationException>(() => 
            CodeBreakerAlgorithms.SelectPeg(44, GameType.Game5x5x4, 4));
    }

    [Theory]
    [InlineData(0b_000100_000100_000100_000100, 0, 0b_000100)]
    [InlineData(0b_000100_000100_000100_000100, 1, 0b_000100)]
    [InlineData(0b_000100_000100_000100_000100, 2, 0b_000100)]
    [InlineData(0b_000100_000100_000100_000100, 3, 0b_000100)]
    public void SelectPegTest_Game6x4(int code, int number, int expected)
    {
        int actual = CodeBreakerAlgorithms.SelectPeg(code, GameType.Game6x4, number);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0b_0100_0100_0100_0100_0100, 0, 0b_0100)]
    [InlineData(0b_0100_0100_0100_0100_0100, 1, 0b_0100)]
    [InlineData(0b_0100_0100_0100_0100_0100, 2, 0b_0100)]
    [InlineData(0b_0100_0100_0100_0100_0100, 3, 0b_0100)]
    [InlineData(0b_0100_0100_0100_0100_0100, 4, 0b_0100)]
    public void SelectPegTest_Game8x5(int code, int number, int expected)
    {
        int actual = CodeBreakerAlgorithms.SelectPeg(code, GameType.Game8x5, number);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0b_00100_00100_00100_00100, 0, 0b_00100)]
    [InlineData(0b_00100_00100_00100_00100, 1, 0b_00100)]
    [InlineData(0b_00100_00100_00100_00100, 2, 0b_00100)]
    [InlineData(0b_00100_00100_00100_00100, 3, 0b_00100)]
    public void SelectPegTest_Game5x5x4(int code, int number, int expected)
    {
        int actual = CodeBreakerAlgorithms.SelectPeg(code, GameType.Game5x5x4, number);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void HandleBlackMatches_Should_Find1BlackMatch_Game6x4()
    {
        List<int> toMatch =
        [
            0b_100000_010000_100000_100000,  // hit
            0b_100000_010000_010000_100000,  // hit
            0b_000100_000100_000100_000100   // miss
        ];
        int selection = 0b_000001_010000_000001_000001;

        List<int> actual = CodeBreakerAlgorithms.HandleBlackMatches(toMatch, GameType.Game6x4, 1, selection);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public void HandleBlackMatches_Should_Find1BlackMatch_Game8x5()
    {
        List<int> toMatch =
        [
            0b_1000_0100_1000_1000_1000,  // hit
            0b_1000_0100_0100_1000_1000,  // hit
            0b_0010_0010_0010_0010_0010   // miss
        ];
        int selection = 0b_0001_0100_0001_0001_0001;

        List<int> actual = CodeBreakerAlgorithms.HandleBlackMatches(toMatch, GameType.Game8x5, 1, selection);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public void HandleBlackMatches_Should_Find2BlackMatches_Game6x4()
    {
        List<int> toMatch =
        [
            0b_100000_010000_010000_100000,  // hit
            0b_000001_100000_010000_100000,  // hit
            0b_000100_010000_000100_000100   // miss
        ];
        int selection = 0b_000001_010000_010000_000001;

        List<int> actual = CodeBreakerAlgorithms.HandleBlackMatches(toMatch, GameType.Game6x4, 2, selection);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public void HandleBlackMatches_Should_Find3BlackMatches_Game6x4()
    {
        List<int> toMatch =
        [
            0b_100000_010000_010000_100000,  // miss
            0b_000001_100000_010000_100000,  // hit
            0b_000100_010000_000100_000100   // miss
        ];
        int selection = 0b_000001_100000_010000_000001;

        List<int> actual = CodeBreakerAlgorithms.HandleBlackMatches(toMatch, GameType.Game6x4, 3, selection);
        Assert.Single(actual);
    }

    [Fact]
    public void HandleBlackMatches_Should_BeEmpty_Game6x4()
    {
        List<int> toMatch =
        [
            0b_000100_010000_001000_000010
        ];
        int selection = 0b_000001_010000_001000_001000;
        List<int> actual = CodeBreakerAlgorithms.HandleBlackMatches(toMatch, GameType.Game6x4, 1, selection);
        Assert.Empty(actual);
    }

    [Fact]
    public void HandleWhiteMatches_Should_Find1WhiteMatches_Game6x4()
    {
        List<int> toMatch =
        [
            0b_010000_100000_100000_100000,  // hit
            0b_010000_100000_010000_100000,  // hit
            0b_000100_000100_000100_000100   // miss
        ];
        int selection = 0b_000001_010000_000001_000001;

        List<int> actual = CodeBreakerAlgorithms.HandleWhiteMatches(toMatch, GameType.Game6x4, 1, selection);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public void IntToColors_Should_ConvertToCorrectColor_Game6x4()
    {
        int value = 0b_000100_010000_000001_100000;
        Dictionary<int, string> colorNames = new()
        {
            { 0b_000001, "Black" },
            { 0b_000010, "White" },
            { 0b_000100, "Red" },
            { 0b_001000, "Green" },
            { 0b_010000, "Blue" },
            { 0b_100000, "Yellow" }
        };
        string[] expected = ["Red", "Blue", "Black", "Yellow"];
        string[] actual = CodeBreakerAlgorithms.IntToColors(value, GameType.Game6x4, colorNames);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void HandleNoMatches_Should_MatchOneResult_Game6x4()
    {
        List<int> toMatch =
        [
            0b_010000_100000_100000_100000,  // miss
            0b_010000_100000_010000_100000,  // miss
            0b_001000_001000_001000_001000   // hit
        ];
        int selection = 0b_000100_010000_000001_100000;
        List<int> actual = CodeBreakerAlgorithms.HandleNoMatches(toMatch, GameType.Game6x4, selection);
        Assert.Single(actual);
    }

    //private IEnumerable<string> _values;
    //public MMAlgorithmsTests()
    //{
    //    _values = new List<string>()
    //    {
    //        "0123", "4512", "5555", "4444", "3423"
    //    };
    //}

    //[Theory]
    //[InlineData("0123", "black", "white", "red", "green")]
    //[InlineData("4501", "blue", "yellow", "black", "white")]
    //public void TestColorNamesStringToColor(string input, params string[] colors)
    //{
    //    (var colorNames, var chars) = MMAlgorithms.StringToColors(input);
    //    Assert.Equal(colors, colorNames);
    //}

    //[Theory]
    //[InlineData("0123", '0', '1', '2', '3')]
    //[InlineData("4501", '4', '5', '0', '1')]
    //public void TestColorNamesStringToNumbers(string input, params char[] charsexpected)
    //{
    //    (var colorNames, var chars) = MMAlgorithms.StringToColors(input);
    //    Assert.Equal(charsexpected, chars);
    //}

    //[Theory]
    //[InlineData(1, 1, '0', '0', '0', '0')]
    //[InlineData(1, 2, '4', '4', '4', '4')]
    //[InlineData(2, 1, '2', '3', '2', '3')]
    //public void TestReducePossibleValues(int hits, int expectedCount, params char[] chars)
    //{
    //    List<string> results = MMAlgorithms.ReducePossibleValues(_values, hits, chars);
    //    Assert.Equal(expectedCount, results.Count);
    //}

    //[Theory]
    //[ClassData(typeof(TestReduceData))]
    //public void TestReducePossibleValuesReturnsHit(string code, int hits, char[] chars)
    //{
    //    List<string> values = new() { code };
    //    var actual = MMAlgorithms.ReducePossibleValues(values, hits, chars);
    //    Assert.True(actual.Count() == 1);
    //}
}

public class TestReduceData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            "4245",
            3,
            new char[] { '4', '5', '4', '1' }
        };
        yield return new object[]
        {
            "2454",
            1,
            new char[] { '1', '4', '3', '1' }
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
