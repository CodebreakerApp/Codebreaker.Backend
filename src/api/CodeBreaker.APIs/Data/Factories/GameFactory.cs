using CodeBreaker.APIs.Data.Factories.GameTypeFactories;
using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Data.Factories;

internal static class GameFactory
{
    public static Game CreateWithRandomCode(string username, GameTypeFactory<string> gameTypeFactory) =>
        CreateWithRandomCode(username, gameTypeFactory.Create());

    public static Game CreateWithRandomCode(string username, GameType<string> gameType) =>
        new(
            Guid.NewGuid(),
            gameType,
            username,
            CodeFactory.CreateRandomCode(gameType),
            DateTime.Now,
            null,
            new List<Move> ()
        );
}
