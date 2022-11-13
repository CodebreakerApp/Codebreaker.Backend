namespace CodeBreaker.Shared.Models.Data.GameTypes;

public abstract class GameTypeBase : IGameTypeVisitable
{
    // name via visitor

    public int Holes { get; init; }

    public int MaxMoves { get; init; }

    public abstract void Accept(IGameTypeVisitor visitor);

    public abstract TResult Accept<TResult>(IGameTypeVisitor<TResult> visitor);
}
