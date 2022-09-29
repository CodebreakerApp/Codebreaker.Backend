using CodeBreaker.Shared.Models.Data;
using static CodeBreaker.Shared.Models.Data.Colors;

namespace CodeBreaker.APIs.Factories.GameTypeFactories;

internal class GameType6x4MiniFactory : GameTypeFactory
{
    public GameType6x4MiniFactory() : base("6x4MiniGame") { }

    public override GameType Create() =>
        new GameType(
            "6x4MiniGame",
            new string[] { Black, White, Red, Green, Blue, Yellow },
            4,
            12
        );
}
