using System.Diagnostics.CodeAnalysis;

namespace Codebreaker.GameAPIs.Models;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Primary constructor warnings with preview, remove suppression with a new preview")]
public abstract class Move(Guid gameId, Guid moveId, int moveNumber)
{
    public Guid GameId => gameId;
    public Guid MoveId => moveId;
    public int MoveNumber => moveNumber;
}

public class Move<TField, TResult>(Guid GameId, Guid MoveId, int MoveNumber)
    : Move(GameId, MoveId, MoveNumber)
{
    public required ICollection<TField> GuessPegs { get; init; }
    public required TResult KeyPegs { get; init; }

    public override string ToString() =>
        $"{MoveNumber}, {string.Join(".", GuessPegs)}";
}
