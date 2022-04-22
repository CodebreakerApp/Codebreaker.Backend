namespace CodeBreaker.Shared;  

public record GameMove(string GameId, int MoveNumber, IList<string> CodePegs)
{
    public override string ToString() => $"{GameId}.{MoveNumber}, {string.Join("..", CodePegs)}";
}

public record GameMoveResult(string GameId, int MoveNumber, bool Completed = false, bool Won = false)
{
    public override string ToString() => $"{string.Join(".", KeyPegs)}";

    public IList<string> KeyPegs { get; } = new List<string>();
}

public record GameStatus(GameMove Move, GameMoveResult Result);

public record Game(string GameId, string Name, IReadOnlyList<string> Code)
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
