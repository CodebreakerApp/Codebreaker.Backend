namespace CodeBreaker.APIs.Data;

public interface ICodeBreakerContext
{
    Task AddGameAsync(CodeBreakerGame game);
    Task AddMoveAsync(CodeBreakerGameMove move);
}
