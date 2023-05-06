namespace CodeBreaker.Common.Models.Data;
public static class GameExtensions
{
    public static bool Ended(this Game game) => game.Endtime is not null;
}
