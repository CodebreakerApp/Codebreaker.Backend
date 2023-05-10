using Codebreaker.GameAPIs.Data;
using Codebreaker.GameAPIs.Exceptions;
using Codebreaker.GameAPIs.Factories;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Game = Codebreaker.GameAPIs.Models.Game;

namespace Codebreaker.GameAPIs.Services;

public class GamesService : IGamesService
{
    private readonly ICodebreakerRepository _dataRepository;
    private readonly IMemoryCache _gameCache;
    private readonly ILogger _logger;
    private readonly IPublishEventService _eventService;
    private readonly MemoryCacheEntryOptions _gameCacheEntryOptions;

    public GamesService(
        ICodebreakerRepository dataRepository,
        IMemoryCache gameCache,
        ILogger<GamesService> logger,
        IPublishEventService eventService,
        IOptions<GameServiceOptions> options
    )
    {
        _dataRepository = dataRepository;
        _gameCache = gameCache;
        _logger = logger;
        _eventService = eventService;

        _gameCacheEntryOptions = new MemoryCacheEntryOptions()
        {
            SlidingExpiration = TimeSpan.FromSeconds(options.Value.GameEntryCacheExpiration)
        };
    }

    // get the game from the cache or the data repository
    public async ValueTask<Game?> GetGameAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _gameCache.GetOrCreateAsync(id, async entry =>
        {
            Game? game = await _dataRepository.GetGameAsync(id, withTracking: false, cancellationToken);
            if (game is null)
            {
                _logger.GameIdNotFound(id);
                return default;
            }
            return game;
        });

    public async Task<IEnumerable<Game>> GetMyGamesAsync(string playerName, CancellationToken cancellationToken = default)
    {
        var games = await _dataRepository.GetMyGamesAsync(playerName, cancellationToken);
        return games;
    }

    public async Task<Game> StartGameAsync(GameType gameType, string playerName, CancellationToken cancellationToken = default)
    {
        Game game = GamesFactory.CreateGame(gameType, playerName);

        _gameCache.Set(game.GameId, game, _gameCacheEntryOptions);
        await _dataRepository.CreateGameAsync(game, cancellationToken);
        _logger.GameStarted(game.ToString());
        // TODO: payload
       //  await _eventService.FireGameCreatedEventAsync(new(game), cancellationToken);
        return game;
    }

    public async Task DeleteGameAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _dataRepository.DeleteGameAsync(id, cancellationToken);
        _gameCache.Remove(id);
        _logger.GameEnded(id.ToString());
    }

    public async Task<Game> SetMoveAsync(Guid gameId, Move move, CancellationToken cancellationToken = default)
    {
        Game game = await _dataRepository.GetGameAsync(gameId, true, cancellationToken) ?? throw new GameNotFoundException($"Game with id {gameId} not found");

        game.ApplyMove(move);

        if (game.Ended())
        {
            _gameCache.Remove(gameId);
        }
        else
        {
            _gameCache.Set(gameId, game, _gameCacheEntryOptions);
        }

        // Always update the game in the game-service database
        await _dataRepository.UpdateGameAsync(game, cancellationToken);

        // TODO: what information is needed in the event?
        // await _eventService.FireMoveCreatedEventAsync(new(gameId, move), cancellationToken);

        return game;
    }

    public Task<Game> GiveUpAsync(Guid gameId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Game>> GetMyRunningGamesAsync(string playerName, CancellationToken cancellationToken = default)
    {
        return await _dataRepository.GetMyRunningGamesAsync(playerName, cancellationToken);
    }

    public async Task<IEnumerable<Game>> GetGamesRankByDateAsync(GameType gameType, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _dataRepository.GetGamesByDateAsync(gameType, date, cancellationToken);      
    }
}

public class GameServiceOptions
{
    public int GameEntryCacheExpiration { get; set; } = 300;
}
