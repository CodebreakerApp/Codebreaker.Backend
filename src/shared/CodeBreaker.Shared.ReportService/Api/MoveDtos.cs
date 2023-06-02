namespace CodeBreaker.Shared.ReportService.Api;

public class MoveDto
{
    public required IReadOnlyList<string> GuessPegs { get; init; }

    public required KeyPegsDto KeyPegs { get; init; }
}

public readonly record struct KeyPegsDto(int Black, int White);
