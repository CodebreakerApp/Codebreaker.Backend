namespace CodeBreaker.APIs.Services
{
    internal interface IGameCache
    {
        void DeleteGame(string id);
        Game? GetGame(string id);
        void SetGame(Game game);
    }
}