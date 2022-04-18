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
}
