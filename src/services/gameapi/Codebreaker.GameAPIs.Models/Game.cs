namespace Codebreaker.GameAPIs.Models;

public abstract class Game(
    Guid gameId,
    GameType gameType,
    string playerName,
    DateTime startTime,
    int holes,
    int maxMoves)
{
    public Guid GameId { get; private set; } = gameId;
    public GameType GameType { get; private set; } = gameType;
    public string PlayerName { get; private set; } = playerName;
    public IReadOnlyList<string> Code { get; init; } = new List<string>();
    public DateTime StartTime { get; private set; } = startTime;
    public DateTime? Endtime { get; set; }
    public int Holes { get; private set; } = holes;
    public int MaxMoves { get; private set; } = maxMoves;
    public bool Completed { get; set; } = false;
    public bool Won { get; set; } = false;
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
