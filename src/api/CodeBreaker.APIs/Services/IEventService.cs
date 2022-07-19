namespace CodeBreaker.APIs.Services;

public interface IEventService
{
    Task FireGameCreatedEventAsync(Game game, CancellationToken token = default);
    Task FireMoveCreatedEventAsync(GameMove move, CancellationToken token = default);
}
