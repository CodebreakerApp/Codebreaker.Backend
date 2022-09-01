using CodeBreaker.APIs.Services.Cache;
using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Services;

public class MoveService : IMoveService
{
	private readonly CodeBreakerContext _dbContext;

	private readonly IGameCache _gameCache;

	private readonly ILogger _logger;

	public MoveService(CodeBreakerContext dbContext, IGameCache gameCache, ILogger<GameService> logger)
	{
		_dbContext = dbContext;
		_gameCache = gameCache;
		_logger = logger;
	}

	public virtual async Task<Game> CreateMoveAsync(Guid gameId, Move move)
	{
		Game game = await LoadGameFromCacheOrDatabase(gameId);
		game.ApplyMove(move);
		await _dbContext.SaveChangesAsync();
		_gameCache.Set(gameId, game);
		return game;
	}

	private async ValueTask<Game> LoadGameFromCacheOrDatabase(Guid gameId) =>
		_gameCache.GetOrDefault(gameId)
		?? await _dbContext.Games.FindAsync(gameId)
		?? throw new GameNotFoundException($"The game with the id {gameId} was not found");
}
