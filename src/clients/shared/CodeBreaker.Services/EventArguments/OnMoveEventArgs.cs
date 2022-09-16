using CodeBreaker.Shared;
using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.Services.EventArguments;

public class OnMoveEventArgs : EventArgs, ILiveEventArgs<Move?>
{
    public Guid GameId { get; init; }

    public Move? Data { get; init; }
}
