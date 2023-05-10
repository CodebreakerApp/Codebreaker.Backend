using CodeBreaker.Shared.Models.Api;

namespace Codebreaker.GameAPIs.Models;

public static class DtoExtensions
{
    public static CreateGameResponse ToCreateGameResponse(this Game game)
    {
        static CreateGameResponse GetColorGameResponse(ColorGame game) => new (game.GameId, game.GameType, game.PlayerName)
        {
            ColorFields = game.Fields
        };

        static CreateGameResponse GetSimpleGameResponse(SimpleGame game) => new(game.GameId, game.GameType, game.PlayerName)
        {
            ColorFields = game.Fields
        };

        static CreateGameResponse GetShapeGameResponse(ShapeGame game) => new(game.GameId, game.GameType, game.PlayerName)
        {
            ShapeFields = game.Fields
        };

        return game switch
        {
            ColorGame g => GetColorGameResponse(g),
            SimpleGame g => GetSimpleGameResponse(g),
            ShapeGame g => GetShapeGameResponse(g),
            _ => throw new InvalidOperationException("invalid game type")
        };
    }

    public static Move ToMove(this SetMoveRequest request)
    {
        if (request.GameType is GameType.Game5x5x4)
        {
            ArgumentNullException.ThrowIfNull(request.ShapeGuessPegs);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(request.ColorGuessPegs);
        }

        return request.GameType switch
        {
             GameType.Game6x4Mini => new Move<ColorField, SimpleColorResult>(request.GameId, Guid.NewGuid(), request.MoveNumber)
             {
                 GuessPegs = request.ColorGuessPegs!.ToArray()
             },
             GameType.Game6x4 => new Move<ColorField, ColorResult>(request.GameId, Guid.NewGuid(), request.MoveNumber)
             {
                 GuessPegs = request.ColorGuessPegs!.ToArray()
             },
             GameType.Game8x5 => new Move<ColorField, ColorResult>(request.GameId, Guid.NewGuid(), request.MoveNumber)
             {
                    GuessPegs = request.ColorGuessPegs!.ToArray()
             },
             GameType.Game5x5x4 => new Move<ShapeAndColorField, ShapeAndColorResult>(request.GameId, Guid.NewGuid(), request.MoveNumber)
             {
                 GuessPegs = request.ShapeGuessPegs!.ToArray()
             },
             _ => throw new InvalidOperationException()
        };
    }

    public static SetMoveResponse ToSetMoveResponse(this Game game)
    {
        static SetMoveResponse GetColorMoveResponse(ColorGame game) => new(game.GameId, game.GameType, game.LastMoveNumber)
        {
            ColorResult = game.Moves.Last().KeyPegs
        };

        static SetMoveResponse GetSimpleMoveResponse(SimpleGame game) => new(game.GameId, game.GameType, game.LastMoveNumber)
        {
            SimpleResult = game.Moves.Last().KeyPegs
        };

        static SetMoveResponse GetShapeMoveResponse(ShapeGame game) => new(game.GameId, game.GameType, game.LastMoveNumber)
        {
            ShapeResult = game.Moves.Last().KeyPegs
        };

        return game switch
        {
            ColorGame g => GetColorMoveResponse(g),
            SimpleGame g => GetSimpleMoveResponse(g),
            ShapeGame g => GetShapeMoveResponse(g),
            _ => throw new InvalidOperationException("invalid game type")
        };
    }

    public static GetGamesRankResponse ToGamesRankResponse(this IEnumerable<Game> games, DateOnly date, GameType gameType)
    {
        return new GetGamesRankResponse(date, gameType)
        {
            Games = games.Select(g => new GameInfo(g.GameId, g.PlayerName, g.StartTime, g.Duration ?? TimeSpan.MaxValue)).ToArray()
        };
    }
}
