using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.Data.Factories;

internal static class CodeFactory
{
    public static IReadOnlyList<TField> CreateRandomCode<TField>(GameType<TField> gameType)
    {
        var pegs = new TField[gameType.Holes];

        for (int i = 0; i < gameType.Holes; i++)
        {
            int index = Random.Shared.Next(gameType.Fields.Count);
            pegs[i] = gameType.Fields[index];
        }

        return pegs;
    }
}