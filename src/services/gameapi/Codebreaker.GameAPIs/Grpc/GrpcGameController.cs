using CodeBreaker.APIs.Factories.GameTypeFactories;
using CodeBreaker.APIs.Services;
using CodeBreaker.Grpc;
using CodeBreaker.Shared.Models.Data;
using CodeBreaker.Shared.Models.Extensions;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

namespace CodeBreaker.APIs.Grpc;

public class GrpcGameController : GrpcGame.GrpcGameBase
{
    private readonly IGameService _gameService;
    private readonly IMoveService _moveService;
    private readonly ILogger _logger;
    private readonly IGameTypeFactoryMapper<string> _gameTypeFactoryMapper;
    
    public GrpcGameController(
        IGameService gameService,
        IMoveService moveService,
        IGameTypeFactoryMapper<string> gameTypeFactoryMapper,
        ILogger<GrpcGameController> logger)
    {
        _gameService = gameService;
        _moveService = moveService;
        _gameTypeFactoryMapper = gameTypeFactoryMapper;
        _logger = logger;
    }
    
    public override async Task<CreateGameReply> CreateGame(CreateGameRequest request, ServerCallContext context)
    {
        GameTypeFactory<string> gameTypeFactory;

        try
        {
            gameTypeFactory = _gameTypeFactoryMapper[request.GameType];
            Game game = await _gameService.CreateAsync(request.Username, gameTypeFactory);

            GameReply gameReply = new()
            {
                GameId = game.GameId.ToString(),
                Start = Timestamp.FromDateTime(game.Start),
                Type = new CodeBreaker.Grpc.GameType
                {
                    Name = game.Type.Name,
                    MaxMoves = game.Type.MaxMoves,
                    Holes = game.Type.Holes
                },
                Username = game.Username
            };
            gameReply.Type.Fields.AddRange(game.Type.Fields);

            return new CreateGameReply
            {
                GameId = request.GameId,
                Game = gameReply
            };
        }
        catch (GameNotFoundException)
        {
            _logger.GameIdNotFound(Guid.Parse(request.GameId));
            throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));
        }
    }

    public override async Task<SetMoveReply> SetMove(SetMoveRequest request, ServerCallContext context)
    {
        Move move = new(0, request.GuessPegs.ToArray());
        Game game = await _moveService.CreateMoveAsync(Guid.Parse(request.GameId), move);

        KeyPegs? keyPegs = game.GetLastKeyPegsOrDefault();
        int numberblacks = keyPegs?.Black ?? 0;
        int numberwhites = keyPegs?.White ?? 0;
        IEnumerable<string> blacks = Enumerable.Repeat("black", numberblacks);
        IEnumerable<string> whites = Enumerable.Repeat("white", numberwhites);
        IEnumerable<string> all = blacks.Union(whites);

        SetMoveReply reply = new()
        {
             GameId = game.GameId.ToString(),
             Ended = game.Ended,
             Won = game.Won
        };
        reply.KeyPegs.AddRange(all);
        return reply;
    }
}
