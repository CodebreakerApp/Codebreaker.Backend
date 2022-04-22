namespace CodeBreaker.APIs.Data;

internal record CodeBreakerGame(string CodeBreakerGameId, string Code, string User, DateTime Time)
{
    public List<CodeBreakerMove> Moves { get; init; } = new();
}

internal record CodeBreakerMove(int MoveNumber, string Moves, string Keys);

internal record CodeBreakerGameMove(string Id, string CodeBreakerGameId, string User, int MoveNumber, string Move, DateTime Time, string Keys, string Code)
{
    // constructor for the initial move
    public CodeBreakerGameMove(string Id, string CodeBreakerGameId, string User, DateTime Time)
        : this(Id, CodeBreakerGameId, User, MoveNumber: 0, Move: string.Empty, Time, Keys: string.Empty, Code: string.Empty) { }
}

internal record GamesInformationDetail(DateTime Date)
{
    public List<CodeBreakerGame> Games { get; init; } = new List<CodeBreakerGame>();
}

internal record GamesInfo(DateTime Time, string User, int NumberMoves);
