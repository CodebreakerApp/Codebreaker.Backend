using Codebreaker.GameAPIs.Models;

namespace Codebreaker.GameAPIs.Models.Tests;

public class SimpleColorResultTests
{
    [Fact]
    public void TestStringFormattableWith4Values()
    {
        string expected = "0:1:2:0";
        ResultValue[] values = { ResultValue.Incorrect, ResultValue.CorrectColor, ResultValue.CorrectPositionAndColor, ResultValue.Incorrect };
        SimpleColorResult simpleResult = new(values);
        string actual = simpleResult.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestStringFormattableWith3Values()
    {
        string expected = "0:1:2";
        ResultValue[] values = { ResultValue.Incorrect, ResultValue.CorrectColor, ResultValue.CorrectPositionAndColor };
        SimpleColorResult simpleResult = new(values);
        string actual = simpleResult.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestSpanFormattableWith4Values()
    {
        ReadOnlySpan<char> expected = "0:1:2:0".AsSpan();
        ResultValue[] values = { ResultValue.Incorrect, ResultValue.CorrectColor, ResultValue.CorrectPositionAndColor, ResultValue.Incorrect };
        SimpleColorResult simpleResult = new(values);
        Span<char> destination = new char[7].AsSpan();
        bool result = simpleResult.TryFormat(destination, out int charsWritten);
        Assert.True(result);
        Assert.Equal(7, charsWritten);
        bool spansEqual = MemoryExtensions.SequenceEqual(expected, destination);
        Assert.True(spansEqual);
    }

    [Fact]
    public void TestParseWith4Values()
    {
        SimpleColorResult expected = new(new ResultValue[]
        {
            ResultValue.Incorrect,
            ResultValue.CorrectColor,
            ResultValue.CorrectPositionAndColor,
            ResultValue.Incorrect
        });
        string input = "0:1:2:0";
        var actual = SimpleColorResult.Parse(input);

        Assert.Equal(expected, actual);
    }
}
