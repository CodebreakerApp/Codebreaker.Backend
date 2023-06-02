using CodeBreaker.Data.ReportService.Models;
using CodeBreaker.Shared.ReportService.Api;
using Riok.Mapperly.Abstractions;

namespace CodeBreaker.ReportService.WebApi.Mapping;

[Mapper]
internal static partial class GeneralMapper
{
    public static IReadOnlyList<T> CreateReadonlyList<T>(this IEnumerable<T> enumerable) =>
        enumerable.ToList();
}

[Mapper]
internal static partial class GameDtoMapper
{
    public static partial GameDto ToDto(this Game game);

    public static partial IQueryable<GameDto> ProjectToDto(this IQueryable<Game> games);

    public static partial MoveDto ToDto(this Move move);

    public static partial KeyPegsDto ToDto(this KeyPegs keyPegs);
}
