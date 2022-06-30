namespace CodeBreaker.APIs.Services;

internal interface IGameService
{
    Task<Game> StartGameAsync(string username, string gameType);
    Task<GameMoveResult> SetMoveAsync(GameMove guess);
}
