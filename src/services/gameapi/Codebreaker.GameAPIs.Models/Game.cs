using System.Runtime.InteropServices;

using Codebreaker.GameAPIs.Extensions;

namespace Codebreaker.GameAPIs.Models;

public abstract class Game(
    Guid gameId,
    GameType gameType,
    string playerName,
    DateTime startTime,
    int holes,
    int maxMoves)
    : IFormattable
{
    public Guid GameId { get; private set; } = gameId;
    public GameType GameType { get; private set; } = gameType;
    public string PlayerName { get; private set; } = playerName;
    public DateTime StartTime { get; private set; } = startTime;
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public int LastMoveNumber { get; set; } = 0;
    public int Holes { get; private set; } = holes;
    public int MaxMoves { get; private set; } = maxMoves;
    public bool Won { get; set; } = false;

    public string ToString(string? format = default, IFormatProvider? formatProvider = default) =>
        $"{GameId}:{GameType} - {StartTime}";

    public override string ToString() => ToString(null, null);
}

public class Game<TField, TResult>(
    Guid gameId,
    GameType gameType,
    string playerName,
    DateTime startTime,
    int holes,
    int maxMoves)
    : Game(gameId, gameType, playerName, startTime, holes, maxMoves),
    ICalculatableGame<TField, TResult>
    where TResult: struct, IParsable<TResult>
{
    // possible fields the player can choose from
    public required IEnumerable<TField> Fields { get; init; }

    // the code to guess
    public required ICollection<TField> Codes { get; init; }

    // the moves the player made
    internal readonly List<ICalculatableMove<TField, TResult>> _moves = new();
    public ICollection<ICalculatableMove<TField, TResult>> Moves => _moves;
}
