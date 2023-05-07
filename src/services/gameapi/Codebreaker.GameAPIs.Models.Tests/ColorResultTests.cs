
namespace Codebreaker.GameAPIs.Models.Tests;

public class ColorResultTests
{
    [Fact]
    public void TestStringFormattable()
    {
        string expected = "1:2";
        ColorResult colorResult = new(Correct: 1, WrongPosition: 2);
        string actual = colorResult.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestStringParsable()
    {
        ColorResult expected = new(Correct: 1, WrongPosition: 2);
        var actual = ColorResult.Parse("1:2");
        Assert.Equal(expected, actual);
    }
}
