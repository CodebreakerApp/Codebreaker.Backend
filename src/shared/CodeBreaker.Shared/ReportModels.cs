namespace CodeBreaker.Shared;

public record GamesInformationDetail(DateTime Date)
{
    public List<CodeBreakerGame> Games { get; init; } = new List<CodeBreakerGame>();
}

public record GamesInfo(DateTime Time, string User, int NumberMoves, string Id);
