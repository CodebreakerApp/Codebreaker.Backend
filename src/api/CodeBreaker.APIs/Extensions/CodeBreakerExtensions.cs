namespace CodeBreaker.APIs.Extensions;

internal static class CodeBreakerExtensions
{
    public static Game ToGame(this CodeBreakerGame dbGame)
    {
        string[] codes = dbGame.Code.Split('.').ToArray().Where(s => s.Length > 0).ToArray();
        return new Game(dbGame.CodeBreakerGameId, dbGame.GameType, dbGame.User, codes, dbGame.ColorList, dbGame.Holes, dbGame.MaxMoves, dbGame.Time);
    }

    public static CodeBreakerGame ToDataGame(this Game game)
    {
        return new CodeBreakerGame(game.GameId, game.GameType, string.Join("..", game.Code), game.Name, game.Holes, game.ColorList.ToArray(), game.MaxMoves, DateTime.Now);
    }
}
