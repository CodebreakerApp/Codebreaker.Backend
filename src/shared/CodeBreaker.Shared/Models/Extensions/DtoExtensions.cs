using CodeBreaker.Shared.Models.Api;

namespace CodeBreaker.Shared.Models.Data;

public static class DtoExtensions
{
    public static GameDto ToDto(this Game game) =>
        new GameDto
        {
            GameId = game.GameId,
            Type = game.Type,
            Code = game.Code,
            Username = game.Username,
            Start = game.Start,
            End = game.End,
            Moves = game.Moves.Select(m => m.ToDto())
        };

    public static MoveDto<TField> ToDto<TField>(this Move<TField> move) =>
        new MoveDto<TField>
        {
            MoveNumber = move.MoveNumber,
            GuessPegs = move.GuessPegs,
            KeyPegs = move.KeyPegs
        };

    public static Game ToModel(this GameDto dto) =>
        new Game
        {
            GameId = dto.GameId,
            Type = dto.Type,
            Code = dto.Code,
            Moves = dto.Moves.Select(m => m.ToModel()).ToList(),
            Start = dto.Start,
            End = dto.End,
            Username = dto.Username
        };

    public static Move ToModel(this MoveDto<string> dto) =>
        new Move
        {
            KeyPegs = dto.KeyPegs,
            GuessPegs = dto.GuessPegs,
            MoveNumber = dto.MoveNumber
        };
}
