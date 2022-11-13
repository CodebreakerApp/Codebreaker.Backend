namespace CodeBreaker.Shared.Models.Data.Moves;

public interface IMoveVisitable : IVisitable
{
    void Accept(IMoveVisitor visitor);

    TResult Accept<TResult>(IMoveVisitor<TResult> visitor);
}
