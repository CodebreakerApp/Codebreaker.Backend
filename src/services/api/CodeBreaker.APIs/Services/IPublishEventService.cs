using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Services;

public interface IPublishEventService
{
    Task FireGameCreatedEventAsync(Game game, CancellationToken token = default);

    Task FireMoveCreatedEventAsync(Move move, CancellationToken token = default);
}
