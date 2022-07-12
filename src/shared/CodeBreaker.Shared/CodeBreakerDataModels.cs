namespace CodeBreaker.Shared;

// EF Core model to store the game data
public record CodeBreakerGame(Guid CodeBreakerGameId, string GameType, string Code, string User, int Holes, string[] ColorList, int MaxMoves, DateTime Time)
{  
    public List<CodeBreakerMove> Moves { get; init; } = new();
}

// EF Core model to store the game move data
public record CodeBreakerMove(Guid CodeBreakerGameId, int MoveNumber, string Moves, string Keys, DateTime Time);
