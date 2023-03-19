using CodeBreaker.APIs.Factories;
using CodeBreaker.Shared.Models.Data;

using Microsoft.Extensions.Caching.Memory;

namespace CodeBreaker.APIs.Services;

public class GameService : IGameService
{
	private readonly ICodeBreakerRepository _dataRepository;
	private readonly IMemoryCache _gameCache;
	private readonly ILogger _logger;
    private readonly IPublishEventService _eventService;
    private readonly TimeSpan _gameEntryCacheExpiration = TimeSpan.FromMinutes(5);

	public GameService(ICodeBreakerRepository dataRepository, IMemoryCache gameCache, ILogger<GameService> logger, IPublishEventService eventService)
	{
		_dataRepository = dataRepository;
		_gameCache = gameCache;
		_logger = logger;
        _eventService = eventService;
	}

    public virtual IAsyncEnumerable<Game> GetByDate(DateTime date) =>
        GetByDate(DateOnly.FromDateTime(date));

    public virtual IAsyncEnumerable<Game> GetByDate(DateOnly date)
    {
        return _dataRepository.GetGamesByDateAsync(date);
    }

    public virtual async ValueTask<Game?> GetAsync(Guid id)
    {
        return await _gameCache.GetOrCreateAsync<Game?>(id, async entry =>
        {
            Game? game = await _dataRepository.GetGameAsync(id, withTracking: false);
            if (game is null)
            {
                _logger.GameIdNotFound(id);
                return null;
            }
            entry.SlidingExpiration = _gameEntryCacheExpiration;
            return game;
        });
    }

    public virtual async Task<Game> CreateAsync(string username, GameTypeFactory<string> gameTypeFactory)
	{
		Game game = GameFactory.CreateWithRandomCode(username, gameTypeFactory);
        MemoryCacheEntryOptions cacheEntryOptions = new()
        {
           SlidingExpiration = _gameEntryCacheExpiration,
        };
        _gameCache.Set(game.GameId, game, cacheEntryOptions);
        await _dataRepository.CreateGameAsync(game);
        _logger.GameStarted(game.ToString());
        await _eventService.FireGameCreatedEventAsync(new(game));
		return game;
	}

	public virtual async Task CancelAsync(Guid id)
	{
        await _dataRepository.CancelGameAsync(id);
		_gameCache.Remove(id);
        _logger.GameEnded(id.ToString());
	}

	public virtual async Task DeleteAsync(Guid id)
	{
        await _dataRepository.DeleteGameAsync(id);
        _gameCache.Remove(id);
        _logger.GameEnded(id.ToString());
    }
}
