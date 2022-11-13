namespace CodeBreaker.Shared.Models.Data.Moves;

public abstract class MoveBase : IMoveVisitable
{
    public IReadOnlyList<string> KeyPegs { get; init; } = new List<string>();

    public abstract void Accept(IMoveVisitor visitor);

    public abstract TResult Accept<TResult>(IMoveVisitor<TResult> visitor);
}
