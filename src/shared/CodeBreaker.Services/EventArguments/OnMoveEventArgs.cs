using CodeBreaker.Shared;

namespace CodeBreaker.Services.EventArguments;

public class OnMoveEventArgs : EventArgs, ILiveEventArgs<GameMove?>
{
    public GameMove? Data { get; init; }
}
