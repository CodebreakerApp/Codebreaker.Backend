namespace CodeBreaker.Shared.Models.Data.Fields;

public abstract class FieldBase : IFieldVisitable
{
    public abstract void Accept(IFieldVisitor visitor);

    public abstract TResult Accept<TResult>(IFieldVisitor<TResult> visitor);
}
