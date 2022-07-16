namespace CodeBreaker.Shared;

public record GamesInformationDetail(DateTime Date)
{
    public List<CodeBreakerGame> Games { get; init; } = new();
}

public record GamesInfo(DateTime Time, string User, int NumberMoves, Guid Id);
