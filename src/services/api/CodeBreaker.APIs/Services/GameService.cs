using CodeBreaker.APIs.Factories;
using CodeBreaker.APIs.Factories.GameTypeFactories;
using CodeBreaker.Shared.Models.Data;

using Microsoft.Extensions.Caching.Memory;

namespace CodeBreaker.APIs.Services;

public class GameService : IGameService
{
	private readonly ICodeBreakerContext _dbContext;

	private readonly IMemoryCache _gameCache;

	private readonly ILogger _logger;

    private readonly IPublishEventService _eventService;

	public GameService(ICodeBreakerContext dbContext, IGameCache gameCache, ILogger<GameService> logger, IPublishEventService eventService)
	{
		_dbContext = dbContext;
		_gameCache = gameCache;
		_logger = logger;
        _eventService = eventService;
	}

    public virtual IAsyncEnumerable<Game> GetByDate(DateTime date) =>
        GetByDate(DateOnly.FromDateTime(date));


    public virtual IAsyncEnumerable<Game> GetByDate(DateOnly date)
    {
        var begin = new DateTime(date.Year, date.Month, date.Day);
        var end = new DateTime(date.Year, date.Month, date.Day).AddDays(1);
        return _dbContext.Games
            .Where(x => x.Start >= begin && x.Start < end)
            .AsAsyncEnumerable();
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
            return game;
        });
    }

	public virtual async Task<Game> CreateAsync(string username, GameTypeFactory<string> gameTypeFactory)
	{
		Game game = GameFactory.CreateWithRandomCode(username, gameTypeFactory);
		_gameCache.Set(game.GameId, game);
        await _dbContext.CreateGameAsync(game);
        _logger.GameStarted(game.ToString());
        await _eventService.FireGameCreatedEventAsync(new (game));
		return game;
	}

	public virtual async Task CancelAsync(Guid id)
	{
        await _dbContext.CancelGameAsync(id);
		_gameCache.Remove(id);
        _logger.GameEnded(id.ToString());
	}

	public virtual async Task DeleteAsync(Guid id)
	{
        await _dbContext.DeleteGameAsync(id);
        _gameCache.Remove(id);
        _logger.GameEnded(id.ToString());
    }
}
