using Codebreaker.GameAPIs.Extensions;

namespace Codebreaker.GameAPIs.Models;

public abstract class Move(Guid gameId, Guid moveId, int moveNumber)
{
    public Guid GameId { get; private set; } = gameId;
    public Guid MoveId { get; private set; } = moveId;
    public int MoveNumber { get; set; } = moveNumber;
}

public class Move<TField, TResult>(Guid GameId, Guid MoveId, int MoveNumber)
    : Move(GameId, MoveId, MoveNumber), IFormattable, ICalculatableMove<TField, TResult>
    where TResult: struct
{
    public required ICollection<TField> GuessPegs { get; init; }
    public TResult? KeyPegs { get; set; }

    public string ToString(string? format = default, IFormatProvider? formatProvider = default) =>
        $"{MoveNumber}, {string.Join(".", GuessPegs)}";

    public override string ToString() => ToString(null, null);
}
