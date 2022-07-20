namespace CodeBreaker.APIs.Services;

public interface IPublishEventService
{
    Task FireGameCreatedEventAsync(Game game, CancellationToken token = default);
    Task FireMoveCreatedEventAsync(GameMove move, CancellationToken token = default);
}
