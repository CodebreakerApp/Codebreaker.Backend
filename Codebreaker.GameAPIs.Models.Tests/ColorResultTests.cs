using CodeBreaker.GameAPIs.Models;

namespace Codebreaker.GameAPIs.Models.Tests;

public class ColorResultTests
{
    [Fact]
    public void TestToString()
    {
        ColorResult result = new(1, 2);
        string actual = result.ToString();
        Assert.Equal("1:2", actual);
    }

    [Fact]
    public void TestParse()
    {
        string input = "1:2";
        ColorResult expected = new(1, 2);
        var actual = ColorResult.Parse(input);
        Assert.Equal(expected, actual);
    }
}