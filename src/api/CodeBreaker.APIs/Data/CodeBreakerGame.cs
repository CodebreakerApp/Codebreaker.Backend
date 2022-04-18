namespace CodeBreaker.APIs.Data;

internal record CodeBreakerGame(string CodeBreakerGameId, string Code, string User, DateTime Time)
{
    public List<CodeBreakerMove> Moves { get; init; } = new();
}

internal record CodeBreakerMove(int MoveNumber, string Moves, string Keys);

internal record CodeBreakerGameMove(string Id, string CodeBreakerGameId, int MoveNumber, string Move, DateTime Time, string Keys, string Code);
