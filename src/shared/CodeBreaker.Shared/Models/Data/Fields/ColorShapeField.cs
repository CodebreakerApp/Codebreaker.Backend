namespace CodeBreaker.Shared.Models.Data.Fields;

public class ColorShapeField : FieldBase
{
    public string Shape { get; init; } = string.Empty;

    public override void Accept(IFieldVisitor visitor) => visitor.Visit(this);

    public override TResult Accept<TResult>(IFieldVisitor<TResult> visitor) => visitor.Visit(this);
}
