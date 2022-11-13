namespace CodeBreaker.Shared.Models.Data.Games;

public interface IGameVisitor : IVisitor
{
    void Visit(Game6x4 game);
}

public interface IGameVisitor<TResult> : IVisitor
{
    TResult Visit(Game6x4 game);
}
