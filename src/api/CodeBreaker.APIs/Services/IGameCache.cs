namespace CodeBreaker.APIs.Services
{
    internal interface IGameCache
    {
        void DeleteGame(Guid id);

        Game? GetGame(Guid id);

        void SetGame(Game game);
    }
}