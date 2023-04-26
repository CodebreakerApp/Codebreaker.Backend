namespace CodeBreaker.Queuing.ReportService.Transfer;

public record class Game(
    Guid Id,
    DateTime Start,
    DateTime? End
);
