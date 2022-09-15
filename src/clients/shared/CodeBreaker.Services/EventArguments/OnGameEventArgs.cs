using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.Services.EventArguments;

public class OnGameEventArgs : EventArgs, ILiveEventArgs<Game?>
{
    public Game? Data { get; init; }
}
