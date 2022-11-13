namespace CodeBreaker.Shared.Models.Data.GameTypes;

public interface IGameTypeVisitor<TResult> : IVisitor
{
    TResult Visit(GameType6x4 field);
}

public interface IGameTypeVisitor : IVisitor
{
    void Visit(GameType6x4 field);
}
