using Codebreaker.GameAPIs.Exceptions;

namespace Codebreaker.GameAPIs.Factories;

public static class GamesFactory
{
    private static readonly string[] s_colors6 = { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple, Colors.Orange };
    private static readonly string[] s_colors8 = { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple, Colors.Orange, Colors.Pink, Colors.Brown };
    private static readonly string[] s_colors5 = { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple };
    private static readonly string[] s_shapes5 = { Shapes.Circle, Shapes.Square, Shapes.Triangle, Shapes.Star, Shapes.Rectangle };

    public static Game CreateGame(GameType gameType, string playerName)
    {
        static string GetRandomValue(string[] items) => items[Random.Shared.Next(items.Length)];

        static SimpleGame Create6x4SimpleGame(GameType gameType, string playerName) =>
            new (Guid.NewGuid(), gameType, playerName, DateTime.Now, 4, 12)
            {
                Fields = s_colors6.Select(c => new ColorField(c)).ToArray(),
                Codes = Enumerable.Range(0, 4).Select(i => new ColorField(GetRandomValue(s_colors6))).ToArray()
            };

        static ColorGame Create6x4Game(GameType gameType, string playerName) =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.Now, 4, 12)
            {
                Fields = s_colors6.Select(c => new ColorField(c)).ToArray(),
                Codes = Enumerable.Range(0, 4).Select(i => new ColorField(GetRandomValue(s_colors6))).ToArray()
            };

        static ColorGame Create8x5Game(GameType gameType, string playerName) =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.Now, 5, 12)
            {
                Fields = s_colors8.Select(c => new ColorField(c)).ToArray(),
                Codes = Enumerable.Range(0, 5).Select(i => new ColorField(GetRandomValue(s_colors8))).ToArray()
            };

        static ShapeGame Create5x5x4Game(GameType gameType, string playerName) =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.Now, 4, 14)
            {
                Fields = Enumerable.Range(0, 5).Select(i => new ShapeAndColorField(s_shapes5[i], s_colors5[i])).ToArray(),
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
