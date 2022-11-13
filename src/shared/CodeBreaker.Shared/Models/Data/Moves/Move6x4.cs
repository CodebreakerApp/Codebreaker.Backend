namespace CodeBreaker.Shared.Models.Data.Moves;

public class Move6x4 : MoveBase
{
    public IReadOnlyList<ColorField> GuessPegs { get; init; } = new List<ColorField>();

    public override void Accept(IMoveVisitor visitor) => visitor.Visit(this);

    public override TResult Accept<TResult>(IMoveVisitor<TResult> visitor) => visitor.Visit(this);
}
