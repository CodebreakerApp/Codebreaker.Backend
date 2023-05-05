namespace CodeBreaker.Data.ReportService.Models;

public class Move
{
    public required IList<string> GuessPegs { get; init; }

    public required KeyPegs KeyPegs { get; init; }
}

public readonly record struct KeyPegs(int Black, int White);
