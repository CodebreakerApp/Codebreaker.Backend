using CodeBreaker.APIs.Data.Factories;
using CodeBreaker.APIs.Data.Factories.GameTypeFactories;
using CodeBreaker.APIs.Services.Cache;
using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Services;

public class GameService : IGameService
{
	private readonly CodeBreakerContext _dbContext;

	private readonly IGameCache _gameCache;

	private readonly ILogger _logger;

    private readonly IPublishEventService _eventService;

	public GameService(CodeBreakerContext dbContext, IGameCache gameCache, ILogger<GameService> logger, IPublishEventService eventService)
	{
		_dbContext = dbContext;
		_gameCache = gameCache;
		_logger = logger;
        _eventService = eventService;
	}

	public virtual IAsyncEnumerable<Game> GetByDate(DateOnly date)
    {
        DateTime begin = new DateTime(date.Year, date.Month, date.Day);
        DateTime end = new DateTime(date.Year, date.Month, date.Day + 1);
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
		_dbContext.Games.Add(game);
		await _dbContext.SaveChangesAsync();
        _logger.GameStarted(game.ToString());
        await _eventService.FireGameCreatedEventAsync(game);
		return game;
	}

	public virtual async Task CancelAsync(Guid id)
	{
		Game? game = await _dbContext.Games.FindAsync(id);
		_gameCache.Remove(id);

		if (game is null)
			return;

		game.End = DateTime.Now;
		await _dbContext.SaveChangesAsync();
	}

	public virtual async Task DeleteAsync(Guid id)
	{
		Game? game = await _dbContext.Games.FindAsync(id);
		_gameCache.Remove(id);

		if (game is null)
			return;

		_dbContext.Games.Remove(game);
		await _dbContext.SaveChangesAsync();
	}
}
