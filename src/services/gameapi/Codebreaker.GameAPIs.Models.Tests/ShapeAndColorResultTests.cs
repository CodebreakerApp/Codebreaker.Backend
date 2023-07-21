
namespace Codebreaker.GameAPIs.Models.Tests;

public class ShapeAndColorResultTests
{ 
    [Fact]
    public void TestStringFormattable()
    {
        string expected = "0:1:1";
        ShapeAndColorResult colorResult = new(Correct: 0, WrongPosition: 1, ColorOrShape: 1);
        string actual = colorResult.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestStringParsable()
    {
        ShapeAndColorResult expected = new(Correct: 0, WrongPosition: 1, ColorOrShape: 1);
        var actual = ShapeAndColorResult.Parse("0:1:1");
        Assert.Equal(expected, actual);
    }
}
