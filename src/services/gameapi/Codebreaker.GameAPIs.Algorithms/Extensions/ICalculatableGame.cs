
namespace Codebreaker.GameAPIs.Extensions;

public interface ICalculatableGame<TField, TResult>
{
    int Holes { get; }
    int MaxMoves { get; }
    DateTime EndTime { get; set; }
    bool Won { get; set; }
    IEnumerable<TField> Fields { get; }
    ICollection<TField> Codes { get; init; }
    ICollection<ICalculatableMove<TField, TResult>> Moves { get; }
}
