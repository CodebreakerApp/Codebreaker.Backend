using CodeBreaker.GameAPIs.Models;

namespace Codebreaker.GameAPIs.Models.Tests;

public class SimpleColorResultTests
{
    [Fact]
    public void TestToString()
    {
        ResultValue[] values = { ResultValue.CorrectColor, ResultValue.CorrectPositionAndColor, ResultValue.Incorrect, ResultValue.Incorrect };
        SimpleColorResult result = new(values);
        string actual = result.ToString();
        Assert.Equal("1:2:0:0", actual);
    }

    [Fact]
    public void TestParse()
    {
        ResultValue[] values = { ResultValue.CorrectColor, ResultValue.CorrectPositionAndColor, ResultValue.Incorrect, ResultValue.Incorrect };
        SimpleColorResult expected = new(values);
        var actual = SimpleColorResult.Parse("1:2:0:0");
        Assert.Equal(expected, actual);
    }
}