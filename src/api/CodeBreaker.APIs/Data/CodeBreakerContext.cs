namespace CodeBreaker.APIs.Data;

internal class CodeBreakerContext : DbContext, ICodeBreakerContext
{
    private readonly ILogger _logger;
    public CodeBreakerContext(DbContextOptions<CodeBreakerContext> options, ILogger<CodeBreakerContext> logger)
        : base(options) 
    {
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("CodeBreakerContainer");
        modelBuilder.Entity<CodeBreakerGame>()
            .HasPartitionKey(g => g.CodeBreakerGameId);
        modelBuilder.Entity<CodeBreakerGameMove>()
            .HasPartitionKey(m => m.CodeBreakerGameId);
    }

    public DbSet<CodeBreakerGame> Games => Set<CodeBreakerGame>();
    public DbSet<CodeBreakerGameMove> Moves => Set<CodeBreakerGameMove>();

    public async Task InitGameAsync(CodeBreakerGame game)
    {
        Games.Add(game);
        await SaveChangesAsync();
        _logger.LogInformation("initialized game with id {gameid}", game.CodeBreakerGameId);
    }
    
    public async Task UpdateGameAsync(CodeBreakerGame game)
    {
        try
        {
            var gameMoves = await Moves
                .Where(m => m.CodeBreakerGameId == game.CodeBreakerGameId)
                .ToListAsync();
            var moves = gameMoves.Select(
                m => new CodeBreakerMove(m.CodeBreakerGameId, m.MoveNumber, m.Move, m.Keys, DateTime.Now))
                .ToList();
            game = game with { Moves = moves };
            Games.Update(game);
            Moves.RemoveRange(gameMoves);
            int records = await SaveChangesAsync();
            _logger.LogInformation("added/updated {records} records", records);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
            throw;
        }
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
            _logger.Error(ex, ex.Message);
            throw;
        }
    }
    
    public async Task<CodeBreakerGame?> GetGameAsync(Guid gameId)
    {
        var game = await Games
            .AsNoTracking()
            .SingleOrDefaultAsync(g => g.CodeBreakerGameId == gameId);
        return game;
    }

    public async Task<GamesInformationDetail> GetGamesDetailsAsync(DateTime date)
    {
        // TODO: .NET 7 - update for DateOnly optimization - EF Core and JSON support needed
        
        var games = await Games
            .AsNoTracking()
            .Where(g => g.Time >= date && g.Time <= date.AddDays(1))
            .Where(g => g.Moves.Count > 0)
            .OrderByDescending(g => g.Time)
            .Take(50)
            .ToListAsync();

        return new GamesInformationDetail(date) { Games = games };        
    }

    public async Task<IEnumerable<GamesInfo>> GetGamesAsync(DateTime date)
    {
        // TODO: .NET 7 - update for DateOnly optimization - EF Core and JSON support needed

        var games = await Games
            .AsNoTracking()
            .Where(g => g.Time >= date && g.Time <= date.AddDays(1))
            .OrderByDescending(g => g.Time)
            .Take(50)
            .Select(g => new { g.Time, g.User, g.Moves, g.CodeBreakerGameId })
            .ToListAsync();

        var games2 = games
            .Where(g => g.Moves.Count > 0)
            .Select(g => new GamesInfo(g.Time, g.User, g.Moves.Count, g.CodeBreakerGameId)).ToList();

        return games2;
    }

    public async Task<CodeBreakerGame?> GetGameDetailAsync(Guid gameId)
    {
        var game = await Games
            .AsNoTracking()
            .SingleOrDefaultAsync(g => g.CodeBreakerGameId == gameId);
        return game;
    }
}
