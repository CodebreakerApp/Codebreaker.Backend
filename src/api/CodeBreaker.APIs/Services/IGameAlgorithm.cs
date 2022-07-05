namespace CodeBreaker.APIs.Services
{
    internal interface IGameAlgorithm
    {
        (GameMoveResult Result, CodeBreakerGame DataGame, CodeBreakerGameMove? Move) SetMove(Game game, GameMove guess);
    }
}