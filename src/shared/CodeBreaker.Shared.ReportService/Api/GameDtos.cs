namespace CodeBreaker.Shared.ReportService.Api;

public record class GameDto(
    Guid Id,
    DateTime Start,
    DateTime? End
);

public record class GetGamesResponse(IAsyncEnumerable<GameDto> Games);

public record class GetGameResponse(GameDto game);
