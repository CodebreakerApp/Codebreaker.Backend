namespace CodeBreaker.Shared;

public record CodeBreakerGame(string CodeBreakerGameId, string Code, string User, DateTime Time)
{
    public List<CodeBreakerMove> Moves { get; init; } = new();
}

public record CodeBreakerMove(string CodeBreakerGameId, int MoveNumber, string Moves, string Keys);
