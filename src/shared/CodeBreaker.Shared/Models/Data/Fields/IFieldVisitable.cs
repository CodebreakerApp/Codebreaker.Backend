namespace CodeBreaker.Shared.Models.Data.Fields;

public interface IFieldVisitable : IVisitable
{
    void Accept(IFieldVisitor visitor);

    TResult Accept<TResult>(IFieldVisitor<TResult> visitor);
}
