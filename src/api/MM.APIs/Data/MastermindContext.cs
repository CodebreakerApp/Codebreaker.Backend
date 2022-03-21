namespace MM.APIs.Data;

public class MastermindContext : DbContext, IMastermindContext
{
    private readonly ILogger _logger;
    public MastermindContext(DbContextOptions<MastermindContext> options, ILogger<MastermindContext> logger)
        : base(options) 
    {
        _logger = logger;
        // this.Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("MastermindContainer");
        modelBuilder.Entity<MasterMindGame>().HasPartitionKey(g => g.MasterMindGameId);
        modelBuilder.Entity<MasterMindGameMove>().HasPartitionKey(m => m.MasterMindGameId);
    }

    public DbSet<MasterMindGame> Games => Set<MasterMindGame>();
    public DbSet<MasterMindGameMove> Moves => Set<MasterMindGameMove>();

    public async Task AddGameAsync(MasterMindGame game)
    {
        var gameMoves = await Moves.Where(m => m.MasterMindGameId == game.MasterMindGameId).ToListAsync();
        var moves = gameMoves.Select(m => new MastermindMove(m.MoveNumber, m.Move, m.Keys)).ToList();
        game = game with { Moves = moves };
        Games.Add(game);
        Moves.RemoveRange(gameMoves);
        int records = await SaveChangesAsync();
        _logger.LogInformation("added/updated {records} records", records);
    }

    public async Task AddMoveAsync(MasterMindGameMove move)
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
