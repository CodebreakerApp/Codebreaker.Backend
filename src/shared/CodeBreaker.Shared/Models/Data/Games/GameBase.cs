using CodeBreaker.Shared.Models.Data.Fields;
using CodeBreaker.Shared.Models.Data.Moves;
using static CodeBreaker.Shared.Models.Data.Colors;

namespace CodeBreaker.Shared.Models.Data.Games;

public abstract class GameBase : IGameVisitable
{
    private readonly Guid _id;

    private readonly string _username = string.Empty;

    private readonly DateTime _start = DateTime.Now;

    private DateTime? _end;

    /// <summary>
    /// The Id of the game.
    /// Used as partitionKey and primaryKey in Cosmos.
    /// </summary>
    public Guid GameId
    {
        get => _id;
        init
        {
            if (value == default)
                throw new ArgumentException(nameof(GameId));

            _id = value;
        }
    }

    public Data.GameTypes.GameTypeBase Type { get; init; }

    public string Username
    {
        get => _username;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(nameof(Username));

            _username = value;
        }
    }

    public IReadOnlyList<FieldBase> Code { get; init; } = new List<FieldBase>();

    public DateTime Start
    {
        get => _start;
        init
        {
            if (value == default)
                throw new ArgumentException(nameof(Start));

            _start = value;
        }
    }

    public DateTime? End
    {
        get => _end;
        set
        {
            if (value < Start)
                throw new ArgumentOutOfRangeException(nameof(End));

            _end = value;
        }
    }

    public virtual bool Ended => End != null;

    public virtual bool Won => Moves.LastOrDefault()?.KeyPegs?.Count(x => x == Black) == Type.Holes;

    public IList<MoveBase> Moves { get; init; } = new List<MoveBase>();

    public abstract void Accept(IGameVisitor visitor);

    public abstract TResult Accept<TResult>(IGameVisitor<TResult> visitor);

    public override string ToString() =>
        $"{GameId} for {Username}, {string.Join("..", Code)}";
}
