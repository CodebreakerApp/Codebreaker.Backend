namespace Codebreaker.GameAPIs.Models;

public partial record ShapeAndColorField(string Shape, string Color)
{
    public override string ToString() => $"{Shape};{Color}";
}
