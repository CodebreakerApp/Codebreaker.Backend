using System.Runtime.CompilerServices;

namespace CodeBreaker.UserService.Services;
internal interface IGamerNameService
{
    Task<bool> IsGamerNameTakenAsync(string gamerName, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> SuggestGamerNamesAsync(int count = 10, [EnumeratorCancellation] CancellationToken cancellationToken = default);
}