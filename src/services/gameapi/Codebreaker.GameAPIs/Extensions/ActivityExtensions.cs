namespace Codebreaker.GameAPIs.Extensions;

public static class ActivityExtensions
{
    public static Activity? StartGame(this ActivitySource activitySource, Guid gameId, string playerName)
    {
        var activity = activitySource.StartActivity("Game started", ActivityKind.Server);
        activity?.AddBaggage("GameId", gameId.ToString());
        activity?.AddBaggage("PlayerName", playerName);
        activity?.AddEvent(new ActivityEvent("Game started"));
        return activity;
    }

    public static Activity? SetMove(this ActivitySource activitySource, Guid gameId, string playerName)
    {
        var activity = activitySource.StartActivity("Move set", ActivityKind.Server);
        activity?.AddBaggage("GameId", gameId.ToString());
        activity?.AddBaggage("PlayerName", playerName);
        activity?.AddEvent(new ActivityEvent("Move set"));
        return activity;
    }
}
