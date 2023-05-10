using CodeBreaker.Shared.Models.Data;
using CodeBreaker.Shared.Models.Live.EventPayloads;

namespace Codebreaker.GameAPIs.Services;

public interface IPublishEventService
{
    Task FireGameCreatedEventAsync(GameCreatedPayload payload, CancellationToken token = default);

    Task FireMoveCreatedEventAsync(MoveCreatedPayload payload, CancellationToken token = default);
}
