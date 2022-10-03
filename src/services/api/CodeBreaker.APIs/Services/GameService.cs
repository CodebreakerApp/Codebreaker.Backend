using CodeBreaker.APIs.Factories;
using CodeBreaker.APIs.Factories.GameTypeFactories;
using CodeBreaker.APIs.Services.Cache;
using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Services;

public class GameService : IGameService
{
	private readonly ICodeBreakerContext _dbContext;

	private readonly IGameCache _gameCache;

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
        DateTime begin = new DateTime(date.Year, date.Month, date.Day);
        DateTime end = new DateTime(date.Year, date.Month, date.Day).AddDays(1);
        return _dbContext.Games
            .Where(x => x.Start >= begin && x.Start < end)
            .AsAsyncEnumerable();
    }

	public virtual async ValueTask<Game?> GetAsync(Guid id) =>
		_gameCache.GetOrDefault(id)
		?? await _dbContext.Games.FindAsync(id);

	public virtual async Task<Game> CreateAsync(string username, GameTypeFactory<string> gameTypeFactory)
	{
		Game game = GameFactory.CreateWithRandomCode(username, gameTypeFactory);
		_gameCache.Set(game.GameId, game);
        await _dbContext.CreateGameAsync(game);
        _logger.GameStarted(game.ToString());
        await _eventService.FireGameCreatedEventAsync(game);
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
