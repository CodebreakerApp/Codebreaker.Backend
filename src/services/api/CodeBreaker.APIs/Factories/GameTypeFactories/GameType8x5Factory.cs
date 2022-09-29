using CodeBreaker.Shared.Models.Data;
using static CodeBreaker.Shared.Models.Data.Colors;

namespace CodeBreaker.APIs.Factories.GameTypeFactories;

internal class GameType8x5Factory : GameTypeFactory
{
    public GameType8x5Factory() : base("8x5Game") { }

    public override GameType Create() =>
        new GameType(
            "8x5Game",
            new string[] { Black, White, Red, Blue, Green, Yellow, Violet, LightBlue },
            5,
            12
        );
}
