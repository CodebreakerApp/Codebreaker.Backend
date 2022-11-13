namespace CodeBreaker.Shared.Models.Data.GameTypes;

public interface IGameTypeVisitable : IVisitable
{
    void Accept(IGameTypeVisitor visitor);

    TResult Accept<TResult>(IGameTypeVisitor<TResult> visitor);
}
