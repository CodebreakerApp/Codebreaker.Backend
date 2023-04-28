using CodeBreaker.Data.ReportService.Models;
using CodeBreaker.Queuing.ReportService.Transfer;

namespace CodeBreaker.ReportService.QueueWorker.Mapping;

internal static class QueueDtoMapping
{
    public static Game ToModel(this GameDto game) =>
        new()
        {
            Id = game.Id,
            Type = game.Type,
            Username = game.Username,
            Start = game.Start,
            End = game.End,
            Moves = game.Moves?.Select(ToModel) as IReadOnlyCollection<Move> ?? Array.Empty<Move>()
        };

    public static Move ToModel(this MoveDto move) =>
        new()
        {
            GuessPegs = move.GuessPegs,
            KeyPegs = move.KeyPegs.ToModel()
        };

    public static KeyPegs ToModel(this KeyPegsDto keyPegs) =>
        new(keyPegs.Black, keyPegs.White);
}
