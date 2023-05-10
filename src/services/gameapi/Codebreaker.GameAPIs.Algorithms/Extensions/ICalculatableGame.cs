
namespace Codebreaker.GameAPIs.Extensions;

public interface ICalculatableGame<TField, TResult>
    where TResult : struct
{
    int Holes { get; }
    int MaxMoves { get; }
    DateTime StartTime { get; }
    DateTime? EndTime { get; set; }
    TimeSpan? Duration { get; set; }
    bool Won { get; set; }
    int LastMoveNumber { get; set; }
    IEnumerable<TField> Fields { get; }
    ICollection<TField> Codes { get; init; }
    ICollection<ICalculatableMove<TField, TResult>> Moves { get; }
}
