using CodeBreaker.Queuing.ReportService.Transfer;

namespace CodeBreaker.Shared.Models.Data;

internal static class ReportServiceMapping
{
    public static GameDto ToReportServiceDto(this Game game) =>
        new(
            game.GameId,
            game.Type.Name,
            game.Username,
            game.Start,
            game.End,
            game.Moves.Select(ToReportServiceDto).ToList()
        );

    public static MoveDto ToReportServiceDto(this Move move) =>
        new(
            move.GuessPegs,
            move.KeyPegs?.ToReportServiceDto() ?? new ()
        );

    public static KeyPegsDto ToReportServiceDto(this KeyPegs keyPegs) =>
        new(keyPegs.Black, keyPegs.White);
}
