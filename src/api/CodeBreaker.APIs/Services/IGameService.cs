namespace CodeBreaker.APIs.Services;

internal interface IGameService
{
    Task<string> StartGameAsync(string username, string gameType);
    Task<GameMoveResult> SetMoveAsync(GameMove guess);
}
