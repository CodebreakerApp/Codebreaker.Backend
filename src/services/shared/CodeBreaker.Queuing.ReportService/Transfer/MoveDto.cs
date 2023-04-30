namespace CodeBreaker.Queuing.ReportService.Transfer;

public record class MoveDto(IReadOnlyList<string> GuessPegs, KeyPegsDto KeyPegs); // IReadonlyCollection<string> cannot not be mapped correctly by EFCore

public readonly record struct KeyPegsDto(int Black, int White);
