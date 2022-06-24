namespace CodeBreaker.Shared;

public class GameTypes
{
    public const string Game6x4 = nameof(Game6x4);
    public const string Game6x4Mini = nameof(Game6x4Mini);
    public const string Game8x5 = nameof(Game8x5);
}

public record CodeBreakerGame(string CodeBreakerGameId, string GameType, string Code, string User, DateTime Time)
{  
    public List<CodeBreakerMove> Moves { get; init; } = new();
}

public record CodeBreakerMove(string CodeBreakerGameId, int MoveNumber, string Moves, string Keys);
