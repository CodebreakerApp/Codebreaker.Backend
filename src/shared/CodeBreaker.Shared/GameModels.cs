namespace CodeBreaker.Shared;

public static class CodeBreakerColors
{
    public const string Black = nameof(Black);
    public const string White = nameof(White);
    public const string Red = nameof(Red);
    public const string Green = nameof(Green);
    public const string Blue = nameof(Blue);
    public const string Yellow = nameof(Yellow);
    public const string Violet = nameof(Violet);
    public const string LightBlue = nameof(LightBlue);

    public const string Rectangle = nameof(Rectangle);
    public const string Circle = nameof(Circle);
    public const string Ellipse = nameof(Ellipse);
    public const string Triangle = nameof(Triangle);
}

public static class GameTypes
{
    public const string Game6x4 = nameof(Game6x4);
    public const string Game6x4Mini = nameof(Game6x4Mini);
    public const string Game8x5 = nameof(Game8x5);
}

public record GameMove(Guid GameId, int MoveNumber, IList<string> GuessPegs)
{
    public override string ToString() => $"{GameId}.{MoveNumber}, {string.Join("..", GuessPegs)}";
}

public record GameMoveResult(Guid GameId, int MoveNumber, bool Completed = false, bool Won = false)
{
    public override string ToString() => $"{string.Join(".", KeyPegs)}";

    public IList<string> KeyPegs { get; } = new List<string>();
}

public record GameStatus(GameMove Move, GameMoveResult Result);

public record Game(Guid GameId, string GameType, string Name, IReadOnlyList<string> Code, IReadOnlyList<string> ColorList, int Holes, int MaxMoves, DateTime StartTime)
{
    public IReadOnlyList<GameStatus> Status { get; } = new List<GameStatus>();
    public override string ToString()
    {
        string codes = string.Join("..", Code);
        return $"{GameId} for {Name}, {codes}";
    }

    public virtual bool Equals(Game? other)
    {
        if (other is null) return false;
        
        if (EqualityContract == other.EqualityContract &&
            GameId == other.GameId &&
            Name == other.Name)
        {
            return Code.SequenceEqual(other.Code);
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode() => GameId.GetHashCode();
}
