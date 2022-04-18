namespace CodeBreaker.APIs.Data;

internal interface ICodeBreakerContext
{
    Task AddGameAsync(CodeBreakerGame game);
    Task AddMoveAsync(CodeBreakerGameMove move);
    Task<GamesInformationDetail> GetGamesDetailsAsync(DateTime date);
    Task<IEnumerable<GamesInfo>> GetGamesAsync(DateTime date);
}
