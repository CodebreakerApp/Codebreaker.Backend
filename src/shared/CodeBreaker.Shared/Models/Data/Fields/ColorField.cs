namespace CodeBreaker.Shared.Models.Data.Fields;

public class ColorField : FieldBase
{
    public string Color { get; init; } = string.Empty;

    public override void Accept(IFieldVisitor visitor) => visitor.Visit(this);

    public override TResult Accept<TResult>(IFieldVisitor<TResult> visitor) => visitor.Visit(this);
}
