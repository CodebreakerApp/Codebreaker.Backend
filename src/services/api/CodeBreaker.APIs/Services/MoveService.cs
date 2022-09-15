using CodeBreaker.APIs.Services.Cache;
using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Services;

public class MoveService : IMoveService
{
	private readonly CodeBreakerContext _dbContext;

	private readonly IGameCache _gameCache;

	private readonly ILogger _logger;

    private readonly IPublishEventService _eventService;

	public MoveService(CodeBreakerContext dbContext, IGameCache gameCache, ILogger<GameService> logger, IPublishEventService eventService)
    {
        _dbContext = dbContext;
        _gameCache = gameCache;
        _logger = logger;
        _eventService = eventService;
    }

    public virtual async Task<Game> CreateMoveAsync(Guid gameId, Move move)
    {
        Game game = await _dbContext.AddMoveAsync(gameId, move);
        await _eventService.FireMoveCreatedEventAsync(move);
        _gameCache.Set(gameId, game);
        _logger.SetMove(move.ToString(), move.KeyPegs?.ToString() ?? string.Empty);
        return game;
    }

    //private async ValueTask<Game> LoadGameFromCacheOrDatabase(Guid gameId) =>
    //    _gameCache.GetOrDefault(gameId)
    //    ?? await _dbContext.GetGameAsync(gameId)
    //    ?? throw new GameNotFoundException($"The game with the id {gameId} was not found");
}
