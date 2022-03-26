namespace CodeBreaker.APIs.Data;

public record CodeBreakerGame(string CodeBreakerGameId, string Code, string User, DateTime Time)
{
    public List<CodeBreakerMove> Moves { get; init; } = new();
}

public record CodeBreakerMove(int MoveNumber, string Moves, string Keys);

public record CodeBreakerGameMove(string Id, string CodeBreakerGameId, int MoveNumber, string Move, DateTime Time, string Keys, string Code);
