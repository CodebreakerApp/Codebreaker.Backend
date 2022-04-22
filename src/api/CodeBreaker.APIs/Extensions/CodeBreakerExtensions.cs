namespace CodeBreaker.APIs.Extensions;

internal static class CodeBreakerExtensions
{
    public static Game ToGame(this CodeBreakerGame dbGame)
    {
        string[] codes = dbGame.Code.Split('.').ToArray().Where(s => s.Length > 0).ToArray();
        return new Game(dbGame.CodeBreakerGameId, dbGame.User, codes);
    }
}
