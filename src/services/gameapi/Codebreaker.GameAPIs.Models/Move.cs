using System.Diagnostics.CodeAnalysis;

namespace Codebreaker.GameAPIs.Models;

public abstract class Move(Guid gameId, Guid moveId, int moveNumber)
{
    public Guid GameId { get; private set; } = gameId;
    public Guid MoveId { get; private set; } = moveId;
    public int MoveNumber { get; private set; } = moveNumber;
}

public class Move<TField, TResult>(Guid GameId, Guid MoveId, int MoveNumber)
    : Move(GameId, MoveId, MoveNumber)
{
    public required ICollection<TField> GuessPegs { get; init; }
    public required TResult KeyPegs { get; init; }

    public override string ToString() =>
        $"{MoveNumber}, {string.Join(".", GuessPegs)}";
}
