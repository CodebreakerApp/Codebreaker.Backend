namespace CodeBreaker.APIs.Data;

internal interface ICodeBreakerContext
{
    // game run
    Task InitGameAsync(CodeBreakerGame game);
    Task UpdateGameAsync(CodeBreakerGame game);
    Task AddMoveAsync(CodeBreakerGameMove move);
    Task<CodeBreakerGame?> GetGameAsync(Guid gameId);

    // report
    Task<GamesInformationDetail> GetGamesDetailsAsync(DateTime date);
    Task<IEnumerable<GamesInfo>> GetGamesAsync(DateTime date);
    Task<CodeBreakerGame?> GetGameDetailAsync(Guid gameId);
}
