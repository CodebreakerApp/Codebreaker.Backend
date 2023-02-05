namespace CodeBreaker.UserService.Services;

internal interface IGamerNameService
{
    Task<bool> CheckGamerNameAsync(string gamerName, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> SuggestGamerNamesAsync(int count = 10, CancellationToken cancellationToken = default);
}
