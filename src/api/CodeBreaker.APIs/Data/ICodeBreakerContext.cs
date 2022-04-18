namespace CodeBreaker.APIs.Data;

internal interface ICodeBreakerContext
{
    Task AddGameAsync(CodeBreakerGame game);
    Task AddMoveAsync(CodeBreakerGameMove move);
}
