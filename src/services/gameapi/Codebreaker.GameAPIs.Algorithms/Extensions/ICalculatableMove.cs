
namespace Codebreaker.GameAPIs.Extensions;

public interface ICalculatableMove<TField, TResult>
{
    int MoveNumber { get; set; }
    ICollection<TField> GuessPegs { get; }
    TResult KeyPegs { get; set; }
}
