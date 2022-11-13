namespace CodeBreaker.Shared.Models.Data.Moves;

public interface IMoveVisitor : IVisitor
{
    void Visit(Move6x4 move);
}

public interface IMoveVisitor<TResult> : IVisitor
{
    TResult Visit(Move6x4 move);
}
