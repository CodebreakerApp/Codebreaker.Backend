namespace Codebreaker.GameAPIs.Models;

public partial record ShapeAndColorField(string Shape, string Color)
{
    public override string ToString() => $"{Shape};{Color}";

    public static implicit operator ShapeAndColorField((string Shape, string Color) f) => new(f.Shape, f.Color);
}
