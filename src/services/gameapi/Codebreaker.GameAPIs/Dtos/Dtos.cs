namespace Codebreaker.GameAPIs.Models;

public record CreateGameRequest(GameType GameType, string PlayerName);

public record CreateGameResponse(Guid GameId, GameType GameType, string PlayerName)
{
    public IEnumerable<ColorField>? ColorFields { get; set; }
    public IEnumerable<ShapeAndColorField>? ShapeFields { get; set; }
}

// depending on the game type, set ColorFields or ShapeAndColorFields
public record SetMoveRequest(Guid GameId, GameType GameType, string PlayerName, int MoveNumber)
{
    public ICollection<ColorField>? ColorGuessPegs { get; set; }
    public ICollection<ShapeAndColorField>? ShapeGuessPegs { get; set; }
}

public record SetMoveResponse(
    Guid GameId,
    GameType GameType,
    int MoveNumber,
    ColorResult? ColorResult = default,
    ShapeAndColorResult? ShapeResult = default,
    SimpleColorResult? SimpleResult = default);

public record GetGameResponse(Game game);

public record GameInfo(Guid GameId, string PlayerName, DateTime StartTime, TimeSpan Duration);

public record GetGamesRankResponse(DateOnly Date, GameType GameType)
{
    public required IEnumerable<GameInfo> Games { get; set; }
}
