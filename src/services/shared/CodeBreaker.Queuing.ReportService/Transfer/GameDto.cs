namespace CodeBreaker.Queuing.ReportService.Transfer;

public record class GameDto(
    Guid Id,
    string Type,
    string Username,
    DateTime Start,
    DateTime? End,
    IReadOnlyCollection<MoveDto> Moves
);
