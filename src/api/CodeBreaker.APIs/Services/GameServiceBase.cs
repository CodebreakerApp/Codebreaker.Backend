//using CodeBreaker.APIs.Data.Factories;
//using CodeBreaker.APIs.Data.Factories.GameTypeFactories;
//using CodeBreaker.Shared.Models.Api;
//using CodeBreaker.Shared.Models.Data;

//namespace CodeBreaker.APIs.Services;

//internal abstract class GameServiceBase<TGameTypeFactory>
//    where TGameTypeFactory : GameTypeFactory
//{
//    private readonly GameTypeFactory _gameTypeFactory;

//    private readonly ILogger _logger;

//    private readonly IGameCache _gameCache;

//    private readonly ICodeBreakerContext _dbContext;

//    public GameServiceBase(GameTypeFactory gameTypeFactory, ILogger logger, IGameCache gameCache, ICodeBreakerContext dbContext)
//    {
//        _gameTypeFactory = gameTypeFactory;
//        _logger = logger;
//        _gameCache = gameCache;
//        _dbContext = dbContext;
//    }

//    public virtual async Task<Game> CreateAsync(CreateGameRequest req)
//    {
//        Game game = GameFactory.CreateWithRandomCode(req.Username, _gameTypeFactory);
//        _gameCache.Cache(game);
//        await _dbContext.InitGameAsync(game);
//    }

//    public virtual async Task<Game> CancelAsync()
//    {

//    }
//}
