using System.Diagnostics.Metrics;

using CodeBreaker.Shared.Models.Api;
using CodeBreaker.Shared.Models.Data;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CodeBreaker.APIs.Endpoints;

public static class GameEndpoints
{
    public static readonly Meter Meter;
    private static readonly Counter<int> s_gamesStarted;
    private static readonly Counter<int> s_movesDone;

    static GameEndpoints()
    {
        Meter = new("CodeBreaker.GameAPI", "2.0.1");
        s_gamesStarted = Meter.CreateCounter<int>("games-started", "games", "the number of games started");
        s_movesDone = Meter.CreateCounter<int>("moves-done", "moves", "the number of moves done");
    }

    public static void MapGameEndpoints(this IEndpointRouteBuilder routes, ILogger logger, ActivitySource activitySource)
    {
        routes.MapGet("/gametypes", (
            [FromServices] IGameTypeFactoryMapper<string> gameTypeFactoryMapper
        ) =>
        {
            IEnumerable<GameType<string>> gameTypes = gameTypeFactoryMapper.GetAllFactories().Select(x => x.Create());
            return TypedResults.Ok(new GetGameTypesResponse(gameTypes.Select(x => x.ToDto())));
        })
        .WithName("GetGameTypes")
        .WithSummary("Gets the available game-types")
        .WithOpenApi()
        .RequireRateLimiting("standardLimiter");

        var group = routes.MapGroup("/games")
            .RequireRateLimiting("standardLimiter")
            .WithTags(nameof(Game));

        group.MapGet("/", (
            [FromQuery] DateTime date,
            [FromServices] IGameService gameService
        ) =>
        {
            IAsyncEnumerable<GameDto> games = gameService
                .GetByDate(date)
                .Select(game => game.ToDto());
            return TypedResults.Ok(new GetGamesResponse(games.ToEnumerable()));
        })
        .WithName("GetGames")
        .WithSummary("Get games by the given date")
        .WithOpenApi(op =>
        {
            op.Parameters[0].Description = "The of date to get the games from. (e.g. 2023-01-01)";
            return op;
        });

        // Get game by id
        group.MapGet("/{gameId:guid}", async Task<Results<Ok<GetGameResponse>, NotFound>> (
            [FromRoute] Guid gameId,
            [FromServices] IGameService gameService
        ) =>
        {
            Game? game = await gameService.GetAsync(gameId);

            if (game is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(new GetGameResponse(game.ToDto()));
        })
        .WithName("GetGame")
        .WithSummary("Gets a game by the given id")
        .WithOpenApi(op =>
        {
            op.Parameters[0].Description = "The id of the game to get";
            return op;
        });

        // Create game
        group.MapPost("/", async Task<Results<Created<CreateGameResponse>, BadRequest<string>>> (
            [FromBody] CreateGameRequest req,
            [FromServices] IGameTypeFactoryMapper<string> gameTypeFactoryMapper,
            [FromServices] IGameService gameService) =>
        {
            GameTypeFactory<string> gameTypeFactory;

            try
            {
                gameTypeFactory = gameTypeFactoryMapper[req.GameType];
            }
            catch (GameTypeNotFoundException)
            {
                logger.GameTypeNotFound(req.GameType);
                return TypedResults.BadRequest("Gametype does not exist");
            }

            Game game = await gameService.CreateAsync(req.Username, gameTypeFactory);

            using var activity = activitySource.StartActivity("Game started", ActivityKind.Server);
            s_gamesStarted.Add(1);
            activity?.AddBaggage("GameId", game.GameId.ToString());
            activity?.AddBaggage("Name", req.Username);
            activity?.AddEvent(new ActivityEvent("Game started"));

            return TypedResults.Created($"/games/{game.GameId}", new CreateGameResponse(game.ToDto()));
        })
        .WithName("CreateGame")
        .WithSummary("Creates and starts a game")
        .WithOpenApi(op =>
        {
            op.RequestBody.Description = "The data of the game to create";
            return op;
        });

        // Cancel or delete game
        group.MapDelete("/{gameId:guid}", async (
            [FromRoute] Guid gameId,
            [FromQuery] bool? cancel,
            [FromServices] IGameService gameService
        ) =>
        {
            if (cancel == false)
                await gameService.DeleteAsync(gameId);
            else
                await gameService.CancelAsync(gameId);

            return TypedResults.NoContent();
        })
        .WithName("CancelOrDeleteGame")
        .WithSummary("Cancels or deletes the game with the given id")
        .WithDescription("A cancelled game remains in the database, whereas a deleted game does not.")
        .WithOpenApi(op =>
        {
            op.Parameters[0].Description = "The id of the game to delete or cancel";
            op.Parameters[1].Description = "Defines whether the game should be cancelled or deleted.";
            return op;
        });

        // Create move for game
        group.MapPost("/{gameId:guid}/moves", async Task<Results<Ok<CreateMoveResponse>, NotFound, BadRequest<string>>> (
            [FromRoute] Guid gameId,
            [FromBody] CreateMoveRequest req,
            [FromServices] IMoveService moveService) =>
        {
            Game game;
            Move move = new(0, req.GuessPegs.ToList(), null);

            try
            {
                game = await moveService.CreateMoveAsync(gameId, move);
            }
            catch (GameNotFoundException)
            {
                return TypedResults.NotFound();
            }

            using var activity = activitySource.StartActivity("Game Move", ActivityKind.Server);
            activity?.AddBaggage("GameId", gameId.ToString());
            s_movesDone.Add(1);

            KeyPegs? keyPegs = game.GetLastKeyPegsOrDefault();

            if (keyPegs is null)
                return TypedResults.BadRequest("Could not get keyPegs");

            return TypedResults.Ok(new CreateMoveResponse(((KeyPegs)keyPegs).ToDto(), game.Ended, game.Won));
        })
        .WithName("CreateMove")
        .WithSummary("Creates a move for the game with the given id")
        .WithOpenApi(op =>
        {
            op.Parameters[0].Description = "The id of the game to create a move for";
            op.RequestBody.Description = "The data for creating the move";
            return op;
        });
    }
}
