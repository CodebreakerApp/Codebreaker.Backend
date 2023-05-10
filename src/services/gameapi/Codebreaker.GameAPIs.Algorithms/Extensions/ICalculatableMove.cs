
namespace Codebreaker.GameAPIs.Extensions;

public interface ICalculatableMove<TField, TResult>
    where TResult: struct
{
    int MoveNumber { get; set; }
    ICollection<TField> GuessPegs { get; }
    TResult? KeyPegs { get; set; }
}
