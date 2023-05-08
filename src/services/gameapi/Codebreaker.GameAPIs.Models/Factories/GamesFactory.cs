using Codebreaker.GameAPIs.Models.Exceptions;

namespace Codebreaker.GameAPIs.Models.Factories;

public static class GamesFactory
{
    private static readonly string[] s_colors6 = { "red", "green", "blue", "yellow", "purple", "orange" };
    private static readonly string[] s_colors8 = { "red", "green", "blue", "yellow", "purple", "orange", "pink", "brown" };
    private static readonly string[] s_colors5 = { "red", "green", "blue", "yellow", "purple" };
    private static readonly string[] s_shapes5 = { "circle", "square", "triangle", "star", "rectangle" };

    public static Game CreateGame(GameType gameType, string playerName)
    {
        static string GetRandomValue(string[] items) => items[Random.Shared.Next(items.Length)];

        static SimpleGame Create6x4SimpleGame(GameType gameType, string playerName) =>
            new (Guid.NewGuid(), gameType, playerName, DateTime.Now, 4, 12)
            {
                Codes = Enumerable.Range(0, 4).Select(i => new ColorField(GetRandomValue(s_colors6))).ToArray()
            };

        static ColorGame Create6x4Game(GameType gameType, string playerName) =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.Now, 4, 12)
            {
                Codes = Enumerable.Range(0, 4).Select(i => new ColorField(GetRandomValue(s_colors6))).ToArray()
            };
        static ColorGame Create8x5Game(GameType gameType, string playerName) =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.Now, 5, 12)
            {
                Codes = Enumerable.Range(0, 5).Select(i => new ColorField(GetRandomValue(s_colors8))).ToArray()
            };

        static ShapeGame Create5x5x4Game(GameType gameType, string playerName) =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.Now, 4, 14)
            {
                Codes = Enumerable.Range(0, 4).Select(i => new ShapeAndColorField(GetRandomValue(s_shapes5), GetRandomValue(s_colors5))).ToArray()
            };

        return gameType switch
        {
            GameType.Game6x4Mini => Create6x4SimpleGame(gameType, playerName),
            GameType.Game6x4 => Create6x4Game(gameType, playerName),
            GameType.Game8x5 => Create8x5Game(gameType, playerName),
            GameType.Game5x5x4 => Create5x5x4Game(gameType, playerName),
            _ => throw new InvalidGameException("Invalid game type") { HResult = 4000 }
        };
    }
}
