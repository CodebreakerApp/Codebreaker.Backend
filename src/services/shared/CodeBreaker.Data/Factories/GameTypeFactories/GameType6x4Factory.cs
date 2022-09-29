using CodeBreaker.Shared.Models.Data;
using static CodeBreaker.Shared.Models.Data.Colors;

namespace CodeBreaker.Data.Factories.GameTypeFactories;

internal class GameType6x4Factory : GameTypeFactory
{
    public GameType6x4Factory() : base("6x4Game") { }

    public override GameType Create() =>
        new GameType(
            "6x4Game",
            new string[] { Black, White, Red, Green, Blue, Yellow },
            4,
            12
        );
}
