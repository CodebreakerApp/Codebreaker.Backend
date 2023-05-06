using System.Diagnostics.CodeAnalysis;

namespace CodeBreaker.Common.Models.Data;

//public abstract record class Game(Guid GameId, string GameType, string PlayerName, int Holes, int MaxMoves);

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Primary constructor warnings with preview, remove suppression with a new preview")]
public abstract class Game(
    Guid gameId,
    GameType gameType,
    string playerName,
    DateTime startTime,
    int holes,
    int maxMoves)
{
    /// <summary>
    /// The Id of the game.
    /// Used as partitionKey and primaryKey in Cosmos.
    /// </summary>
    public Guid GameId => gameId;
    public GameType GameType => gameType;

    public string Playername => playerName;

    public IReadOnlyList<string> Code { get; init; } = new List<string>();

    public DateTime StartTime => startTime;

    public DateTime? Endtime { get; set; }

    public int Holes => holes;

    public int MaxMoves => maxMoves;
}

public class Game<TField, TResult>(
    Guid gameId,
    GameType gameType,
    string playerName,
    DateTime startTime,
    int holes,
    int maxMoves)
    : Game(gameId, gameType, playerName, startTime, holes, maxMoves)
    where TResult : IParsable<TResult>
{
    public required ICollection<TField> Codes { get; init; }
    internal readonly List<Move<TField, TResult>> _moves = new();
    public IEnumerable<Move<TField, TResult>> Moves => _moves;
}
