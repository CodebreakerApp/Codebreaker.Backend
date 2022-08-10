using CodeBreaker.Shared;

namespace CodeBreaker.Services.EventArguments;

public class OnGameEventArgs : EventArgs, ILiveEventArgs<Game?>
{
    public Game? Data { get; init; }
}
