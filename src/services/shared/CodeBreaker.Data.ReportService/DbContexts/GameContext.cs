using CodeBreaker.Data.ReportService.DbConfiguration;
using CodeBreaker.Data.ReportService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeBreaker.Data.ReportService.DbContexts;

public class GameContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <remarks>
    /// See <see href="https://aka.ms/efcore-docs-dbcontext">DbContext lifetime, configuration, and initialization</see> and
    /// <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
    /// </remarks>
    public GameContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("ReportContainer");
        modelBuilder.ApplyConfiguration(new GameDbConfiguration());
    }

    /// <summary>
    /// Gets the game-entities.
    /// </summary>
    /// <value>
    /// The game-entities.
    /// </value>
    public DbSet<Game> Entities => Set<Game>();
}
