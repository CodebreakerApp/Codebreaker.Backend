namespace CodeBreaker.APIs.Data;

internal class CodeBreakerContext : DbContext, ICodeBreakerContext
{
    private readonly ILogger _logger;
    public CodeBreakerContext(DbContextOptions<CodeBreakerContext> options, ILogger<CodeBreakerContext> logger)
        : base(options) 
    {
        _logger = logger;
        // this.Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("CodeBreakerContainer");
        modelBuilder.Entity<CodeBreakerGame>().HasPartitionKey(g => g.CodeBreakerGameId);
        modelBuilder.Entity<CodeBreakerGameMove>().HasPartitionKey(m => m.CodeBreakerGameId);
    }

    public DbSet<CodeBreakerGame> Games => Set<CodeBreakerGame>();
    public DbSet<CodeBreakerGameMove> Moves => Set<CodeBreakerGameMove>();

    public async Task AddGameAsync(CodeBreakerGame game)
    {
        var gameMoves = await Moves.Where(m => m.CodeBreakerGameId == game.CodeBreakerGameId).ToListAsync();
        var moves = gameMoves.Select(m => new CodeBreakerMove(m.MoveNumber, m.Move, m.Keys)).ToList();
        game = game with { Moves = moves };
        Games.Add(game);
        Moves.RemoveRange(gameMoves);
        int records = await SaveChangesAsync();
        _logger.LogInformation("added/updated {records} records", records);
    }

    public async Task AddMoveAsync(CodeBreakerGameMove move)
    {
        try
        {
            Moves.Add(move);
            await SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task<GamesInformationDetail> GetGamesDetailsAsync(DateTime date)
    {
        // TODO: .NET 7 - update for DateOnly optimization - EF Core and JSON support needed
        
        var games = await Games.AsNoTracking()
            .Where(g => g.Time >= date && g.Time <= date.AddDays(1))
            .OrderByDescending(g => g.Time)
            .ToListAsync();

        return new GamesInformationDetail(date) { Games = games };        
    }

    public async Task<IEnumerable<GamesInfo>> GetGamesAsync(DateTime date)
    {
        // TODO: .NET 7 - update for DateOnly optimization - EF Core and JSON support needed

        var games = await Games.AsNoTracking()
            .Where(g => g.Time >= date && g.Time <= date.AddDays(1))
            .OrderByDescending(g => g.Time)
            .Select(g => new { g.Time, g.User, g.Moves })
            .ToListAsync();

        var games2 = games.Select(g => new GamesInfo(g.Time, g.User, g.Moves.Count)).ToList();

        return games2;
    }
}
