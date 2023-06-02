namespace CodeBreaker.Shared.ReportService.Api;

public class GameDto
{
    public required Guid Id { get; init; }

    public required string Type { get; init; }

    public required string Username { get; init; }

    public required DateTime Start { get; init; }

    public required DateTime? End { get; init; }

    public required IReadOnlyList<MoveDto> Moves { get; init; }
}
