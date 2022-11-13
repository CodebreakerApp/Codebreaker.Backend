namespace CodeBreaker.Shared.Models.Data.Fields;

public interface IFieldVisitor : IVisitor
{
    void Visit(ColorField field);

    void Visit(ColorShapeField field);
}

public interface IFieldVisitor<TResult> : IVisitor
{
    TResult Visit(ColorField field);

    TResult Visit(ColorShapeField field);
}
