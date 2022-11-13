namespace CodeBreaker.Shared.Models.Data.Games;

public class Game6x4 : GameBase
{
    public override void Accept(IGameVisitor visitor) => visitor.Visit(this);

    public override TResult Accept<TResult>(IGameVisitor<TResult> visitor) => visitor.Visit(this);
}
