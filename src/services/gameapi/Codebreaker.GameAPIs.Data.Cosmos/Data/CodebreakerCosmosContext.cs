using System.Globalization;

using Codebreaker.GameAPIs.Exceptions;
using Codebreaker.GameAPIs.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Codebreaker.GameAPIs.Data.Cosmos.Data;

public class ColumnNames
{
    public const string Discriminator = nameof(Discriminator);
    public const string StartDate = nameof(StartDate);
}

public class CodebreakerCosmosContext : DbContext, ICodebreakerRepository
{
    private readonly ILogger _logger;

    public CodebreakerCosmosContext(DbContextOptions<CodebreakerCosmosContext> options, ILogger<CodebreakerCosmosContext> logger) : base(options)
    {
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {       
        modelBuilder.HasDefaultContainer("GamesContainer3");

        modelBuilder.Entity<Game>().HasKey(g => g.GameId);
        modelBuilder.Entity<Game>().Property<string>(ColumnNames.StartDate);
        modelBuilder.Entity<Game>().HasPartitionKey(ColumnNames.StartDate);

        modelBuilder.ApplyConfiguration<SimpleGame>(new GameConfiguration<SimpleGame>());
        modelBuilder.ApplyConfiguration<ColorGame>(new GameConfiguration<ColorGame>());
        modelBuilder.ApplyConfiguration<ShapeGame>(new GameConfiguration<ShapeGame>());
        modelBuilder.Entity<Game>().HasDiscriminator<string>(ColumnNames.Discriminator)
            .HasValue<SimpleGame>("Simple")
            .HasValue<ColorGame>("Color")
            .HasValue<ShapeGame>("Shape");
    }

    public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
    {
        // StartData is a shadow property which is used as the partition key
        if (entity is Game g)
        {
            var entry = Entry(entity);
            entry.Property(ColumnNames.StartDate).CurrentValue = DateOnly.FromDateTime(g.StartTime).ToString(CultureInfo.InvariantCulture);
        }
        return base.Add(entity);
    }

    public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default)
    {
        // StartData is a shadow property which is used as the partition key
        if (entity is Game g)
        {
            var entry = Entry(entity);
            entry.Property(ColumnNames.StartDate).CurrentValue = DateOnly.FromDateTime(g.StartTime).ToString(CultureInfo.InvariantCulture);
        }
        return base.AddAsync(entity, cancellationToken);
    }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<ColorGame> GamesColor => Set<ColorGame>();
    public DbSet<ShapeGame> GamesShapes => Set<ShapeGame>();
    public DbSet<SimpleGame> GamesSimple => Set<SimpleGame>();

    public async Task CreateGameAsync(Game game, CancellationToken cancellationToken = default)
    {
        Add(game);
//        Games.Add(game);
        await SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created game with id {gameId}", game.GameId);
    }

    public async Task UpdateGameAsync(Game game, CancellationToken cancellationToken = default)
    {
        Games.Update(game);
        await SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated game with id {gameId}", game.GameId);
    }

    public async Task DeleteGameAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        Game game = await GetGameAsync(gameId, false, cancellationToken) ?? throw new GameNotFoundException($"Game with id {gameId} not found");
        Games.Remove(game);
        await SaveChangesAsync(cancellationToken);
    }

    public Task<Game?> GetGameAsync(Guid gameId, bool withTracking = true, CancellationToken cancellationToken = default) =>
        Games
            .AsTracking(withTracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking)
            .WithPartitionKey(gameId.ToString())
            .SingleOrDefaultAsync(g => g.GameId == gameId, cancellationToken);

    public async Task<IEnumerable<Game>> GetGamesByDateAsync(GameType gameType, DateOnly date, CancellationToken cancellationToken = default)
    {
        // use the partition key to filter by date
        var games = await Games.AsNoTracking()
            .WithPartitionKey(date.ToString())
            .Where(g => g.Won == true)
            .Where(g => g.GameType == gameType)
            .OrderBy(g => g.Duration)
            .Take(100)
            .ToListAsync(cancellationToken);

        return games;
    }

    public async Task<IEnumerable<Game>> GetMyGamesAsync(string playerName, CancellationToken cancellationToken = default)
    {
        var games = await Games.AsNoTracking()
            .Where(g => g.PlayerName == playerName)
            .OrderByDescending(g => g.StartTime)
            .Take(100)
            .ToListAsync(cancellationToken);

        return games;
    }

    public async Task<IEnumerable<Game>> GetMyRunningGamesAsync(string playerName, CancellationToken cancellationToken = default)
    {
        DateTime end = DateTime.Now;
        DateTime start = end.AddDays(-3);

        var games = await Games.AsNoTracking()
            .Where(g => g.PlayerName == playerName)
            .Where(g => g.EndTime == null)
            .Where(g => g.StartTime > start && g.StartTime < end)
            .OrderByDescending(g => g.StartTime)
            .Take(40)
            .ToListAsync(cancellationToken);

        return games;
    }
}
