using CodeBreaker.APIs.Data.DbConfiguration;
using CodeBreaker.Shared.Models.Data;
using CodeBreaker.Shared.Models.Report;

namespace CodeBreaker.APIs.Data;

public class CodeBreakerContext : DbContext//, ICodeBreakerContext
{
    private readonly ILogger _logger;

    public CodeBreakerContext(DbContextOptions<CodeBreakerContext> options, ILogger<CodeBreakerContext> logger) : base(options) 
    {
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("GameContainer");// "CodeBreakerContainer");
        //modelBuilder.Entity<CodeBreakerGame>()
        //    .HasPartitionKey(g => g.CodeBreakerGameId);
        //modelBuilder.Entity<CodeBreakerGameMove>()
        //    .HasPartitionKey(m => m.CodeBreakerGameId);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDbConfiguration).Assembly);
    }

    public DbSet<Game> Games => Set<Game>();

    //public DbSet<Move> Moves => Set<Move>();

    //public DbSet<CodeBreakerGame> Games => Set<CodeBreakerGame>();

    //public DbSet<CodeBreakerGameMove> Moves => Set<CodeBreakerGameMove>();

    public Task InitGameAsync(Game game)
    {
        throw new NotImplementedException();

        //Games.Add(game);
        //await SaveChangesAsync();
        //_logger.LogInformation("initialized game with id {gameid}", game.CodeBreakerGameId);
    }
    
    public Task UpdateGameAsync(Game game)
    {
        throw new NotImplementedException();
        //try
        //{
        //    var gameMoves = await Moves
        //        .Where(m => m.GameId == game.CodeBreakerGameId)
        //        .ToListAsync();
        //    var moves = gameMoves.Select(
        //        m => new CodeBreakerMove(m.CodeBreakerGameId, m.MoveNumber, m.Move, m.Keys, DateTime.Now))
        //        .ToList();
        //    game = game with { Moves = moves };
        //    Games.Update(game);
        //    Moves.RemoveRange(gameMoves);
        //    int records = await SaveChangesAsync();
        //    _logger.LogInformation("added/updated {records} records", records);
        //}
        //catch (Exception ex)
        //{
        //    _logger.Error(ex, ex.Message);
        //    throw;
        //}
    }

    public Task AddMoveAsync(Shared.Models.Data.Move move)
    {
        throw new NotImplementedException();
        //try
        //{
        //    Moves.Add(move);
        //    await SaveChangesAsync();
        //}
        //catch (Exception ex)
        //{
        //    _logger.Error(ex, ex.Message);
        //    throw;
        //}
    }
    
    public Task<Shared.Models.Data.Game?> GetGameAsync(Guid gameId)
    {
        throw new NotImplementedException();
        //var game = await Games
        //    .AsNoTracking()
        //    .SingleOrDefaultAsync(g => g.Id == gameId);
        //return game;
    }

    public Task<GamesInformationDetail> GetGamesDetailsAsync(DateTime date)
    {
        throw new NotImplementedException();

        // TODO: .NET 7 - update for DateOnly optimization - EF Core and JSON support needed
        
        //var games = await Games
        //    .AsNoTracking()
        //    .Where(g => g.StartTimestamp >= date && g.StartTimestamp <= date.AddDays(1))
        //    .Where(g => g.Moves.Count > 0)
        //    .OrderByDescending(g => g.StartTimestamp)
        //    .Take(50)
        //    .ToListAsync();

        //return new GamesInformationDetail(date) { Games = games };        
    }

    public Task<IEnumerable<GamesInfo>> GetGamesAsync(DateTime date)
    {
        throw new NotImplementedException();

        // TODO: .NET 7 - update for DateOnly optimization - EF Core and JSON support needed

        //var games = await Games
        //    .AsNoTracking()
        //    .Where(g => g.StartTimestamp >= date && g.StartTimestamp <= date.AddDays(1))
        //    .OrderByDescending(g => g.StartTimestamp)
        //    .Take(50)
        //    .Select(g => new { g.StartTimestamp, g.Username, g.Moves, g.Id})
        //    .ToListAsync();

        //var games2 = games
        //    .Where(g => g.Moves.Count > 0)
        //    .Select(g => new GamesInfo(g.Time, g.User, g.Moves.Count, g.CodeBreakerGameId)).ToList();

        //return games2;
    }

    public Task<Shared.Models.Data.Game?> GetGameDetailAsync(Guid gameId)
    {
        throw new NotImplementedException();

        //var game = await Games
        //    .AsNoTracking()
        //    .SingleOrDefaultAsync(g => g.Id == gameId);
        //return game;
    }
}
