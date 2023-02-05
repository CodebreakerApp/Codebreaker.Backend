namespace CodeBreaker.UserService.Services;

internal interface IGamerNameService
{
    Task<bool> CheckGamerNameAsync(string gamerName);
    IAsyncEnumerable<string> SuggestGamerNamesAsync(int count = 10);
}
