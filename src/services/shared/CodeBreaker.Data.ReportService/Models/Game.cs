using CodeBreaker.Data.Common.Models;

namespace CodeBreaker.Data.ReportService.Models;

public class Game : IIdentifyable<Guid>
{
    public required Guid Id { get; init; }

    public required string Type { get; init; }

    public required string Username { get; init; }

    public required DateTime Start { get; init; }

    public required DateTime? End { get; init; }

    public required IReadOnlyCollection<Move> Moves { get; init; }
}
