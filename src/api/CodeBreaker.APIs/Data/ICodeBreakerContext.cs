using CodeBreaker.Shared.Models.Data;
using CodeBreaker.Shared.Models.Report;

namespace CodeBreaker.APIs.Data;

internal interface ICodeBreakerContext
{
    public DbSet<Shared.Models.Data.Game> Games { get; }

    public DbSet<Shared.Models.Data.Move> Moves { get; }

    //// game run
    //Task InitGameAsync(CodeBreakerGame game);
    //Task UpdateGameAsync(CodeBreakerGame game);
    //Task AddMoveAsync(CodeBreakerGameMove move);
    //Task<CodeBreakerGame?> GetGameAsync(Guid gameId);

    //// report
    //Task<GamesInformationDetail> GetGamesDetailsAsync(DateTime date);
    //Task<IEnumerable<GamesInfo>> GetGamesAsync(DateTime date);
    //Task<CodeBreakerGame?> GetGameDetailAsync(Guid gameId);
}
