namespace CodeBreaker.Shared.Models.Data.Games;

public interface IGameVisitable : IVisitable
{
    void Accept(IGameVisitor visitor);

    TResult Accept<TResult>(IGameVisitor<TResult> visitor);
}
