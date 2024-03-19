using System.Text;

namespace Codebreaker.GameAPIs.Client.Models;

/// <summary>
/// Filter based on game type, player name, date, and ended
/// </summary>
/// <param name="GameType">The game type with one of the <see cref="GameType"/>enum values</param>
/// <param name="PlayerName">The name of the player</param>
/// <param name="Date">The start time of the game</param>
/// <param name="Ended">Only ended or running games</param>
public record class GamesQuery(
        GameType? GameType = default,
        string? PlayerName = default,
        DateOnly? Date = default,
        bool? Ended = false)
{
    public string AsUrlQuery()
    {
        var queryString = new StringBuilder('?');

        void AppendQueryParameter(string part)
        {
            if (queryString.Length > 1) queryString.Append('&');
            queryString.Append(part);
        }

        // Add condition for gameType
        if (GameType != null)
            AppendQueryParameter($"gameType={GameType}");

        // Add condition for playerName
        if (PlayerName != null)
            AppendQueryParameter($"playerName={Uri.EscapeDataString(PlayerName)}");

        // Add condition for date
        if (Date != null)
            AppendQueryParameter($"date={Date?.ToString("yyyy-MM-dd")}");

        // Add condition for ended
        if (Ended != null)
            AppendQueryParameter($"ended={Ended}");

        // Remove the leading question mark if there are no query parameters
        if (queryString.Length == 1)
            return string.Empty;

        return queryString.ToString();
    }
}
