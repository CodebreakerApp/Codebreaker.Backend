using CodeBreaker.Queuing.ReportService.Services;
using CodeBreaker.Shared.Models.Data;

using Microsoft.Extensions.Caching.Memory;

namespace CodeBreaker.APIs.Services;

public class MoveService : IMoveService
{
    private readonly ICodeBreakerRepository _repository;
	private readonly IMemoryCache _gameCache;
	private readonly ILogger _logger;
    private readonly IPublishEventService _eventService;
    private readonly IGameQueuePublisherService _reportGamePublisherService;


    public MoveService(
        ICodeBreakerRepository repository,
        IMemoryCache gameCache,
        ILogger<GameService> logger,
        IPublishEventService eventService,
        IGameQueuePublisherService reportGamePublisherService
    )
    {
        _repository = repository;
        _gameCache = gameCache;
        _logger = logger;
        _eventService = eventService;
        _reportGamePublisherService = reportGamePublisherService;
    }

    public virtual async Task<Game> CreateMoveAsync(Guid gameId, Move move, CancellationToken cancellationToken = default)
    {
        Game game = await _repository.GetGameAsync(gameId) ?? throw new GameNotFoundException($"Game with id {gameId} not found");
        game.ApplyMove(move);

        if (game.Ended)
        {
            await _reportGamePublisherService.EnqueueMessageAsync(game.ToReportServiceDto(), cancellationToken);
            _gameCache.Remove(gameId);
        }
        else
        {
            _gameCache.Set(gameId, game);
        }

        // Always update the game in the game-service database
        await _repository.UpdateGameAsync(game);

        await _eventService.FireMoveCreatedEventAsync(new(gameId, move), cancellationToken);
        _logger.SetMove(move.ToString() ?? string.Empty, move.KeyPegs?.ToString() ?? string.Empty);

        return game;
    }
}
