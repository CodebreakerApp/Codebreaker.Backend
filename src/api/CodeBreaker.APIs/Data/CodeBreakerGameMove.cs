namespace CodeBreaker.APIs.Data;

internal record CodeBreakerGameMove(string Id, string CodeBreakerGameId, int MoveNumber, string Move, DateTime Time, string Keys, string Code)
{
    // constructor for the initial move
    public CodeBreakerGameMove(string Id, string CodeBreakerGameId, DateTime Time)
        : this(Id, CodeBreakerGameId, MoveNumber: 0, Move: string.Empty, Time, Keys: string.Empty, Code: string.Empty) { }
}
