using CodeBreaker.Data.Factories;
using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.Data.Extensions
{
    internal static class GameTypeExtensions
    {
        public static IEnumerable<string> CreateRandomCode<TColor>(this GameType gameType)
            => CodeFactory.CreateRandomCode(gameType);
    }
}
