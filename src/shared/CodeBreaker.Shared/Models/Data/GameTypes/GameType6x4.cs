namespace CodeBreaker.Shared.Models.Data.GameTypes;

public class GameType6x4 : GameTypeBase
{
    public IReadOnlyList<ColorField> Fields { get; init; } = new List<ColorField>();

    public override void Accept(IGameTypeVisitor visitor) => visitor.Visit(this);

    public override TResult Accept<TResult>(IGameTypeVisitor<TResult> visitor) => visitor.Visit(this);
}
