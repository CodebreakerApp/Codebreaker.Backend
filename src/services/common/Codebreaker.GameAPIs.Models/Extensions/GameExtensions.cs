using System.Runtime.CompilerServices;

namespace Codebreaker.GameAPIs.Extensions;

public static class GameExtensions
{
    [Obsolete("Use HasEnded instead")]
    public static bool Ended(this Game game) => HasEnded(game);
    public static bool HasEnded(this Game game) => game.EndTime != null;

    public static GameSummary ToGameSummary(this Game game) => 
        new(
            game.Id, 
            game.GameType, 
            game.PlayerName, 
            game.HasEnded(), 
            game.IsVictory, 
            game.LastMoveNumber, 
            game.StartTime, 
            game.Duration ?? TimeSpan.MaxValue);
}
