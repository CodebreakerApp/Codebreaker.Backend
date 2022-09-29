using System;
using CodeBreaker.Data.DbConfiguration;
using CodeBreaker.Data.Extensions;
using CodeBreaker.Shared.Exceptions;
using CodeBreaker.Shared.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeBreaker.Data;

public class CodeBreakerContext : DbContext, ICodeBreakerContext
{
    private readonly ILogger _logger;

    public CodeBreakerContext(DbContextOptions<CodeBreakerContext> options, ILogger<CodeBreakerContext> logger) : base(options)
    {
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("GameContainer");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDbConfiguration).Assembly);
    }

    public DbSet<Game> Games => Set<Game>();

    public async Task CreateGameAsync(Game game)
    {
        Games.Add(game);
        await SaveChangesAsync();
        _logger.LogInformation("Created game with id {gameId}", game.GameId);
    }

    public async Task UpdateGameAsync(Game game)
    {
        Games.Update(game);
        await SaveChangesAsync();
        _logger.LogInformation("Updated game with id {gameId}", game.GameId);
    }

    public async Task DeleteGameAsync(Guid gameId)
    {
        Game game = await GetGameAsync(gameId) ?? throw new GameNotFoundException($"Game with id {gameId} not found");
        Games.Remove(game);
        await SaveChangesAsync();
    }

    public async Task CancelGameAsync(Guid gameId)
    {
        Game game = await GetGameAsync(gameId) ?? throw new GameNotFoundException($"Game with id {gameId} not found");
        game.End = DateTime.Now;
        await SaveChangesAsync();
    }

    [Obsolete("Move ApplyMove to API")]
    public async Task<Game> AddMoveAsync(Guid gameId, Move move)
    {
        Game game = await GetGameAsync(gameId) ?? throw new GameNotFoundException($"Game with id {gameId} not found");
        return await AddMoveAsync(game, move);
    }

    [Obsolete("Move ApplyMove to API")]
    public async Task<Game> AddMoveAsync(Game game, Move move)
    {
        game.ApplyMove(move);
        await SaveChangesAsync();
        _logger.LogInformation("Added move to game with id {gameId}", game.GameId);
        return game;
    }

    public Task<Game?> GetGameAsync(Guid gameId) =>
        Games.SingleOrDefaultAsync(g => g.GameId == gameId);

    public IAsyncEnumerable<Game> GetGamesByDateAsync(DateTime date) =>
        GetGamesByDateAsync(DateOnly.FromDateTime(date));

    public IAsyncEnumerable<Game> GetGamesByDateAsync(DateOnly date)
    {
        DateTime begin = new (date.Year, date.Month, date.Day);
        DateTime end = new (date.Year, date.Month, date.Day + 1);

        return Games
            .AsNoTracking()
            .Where(x => x.Start >= begin && x.Start < end)
            .OrderByDescending(x => x.Start)
            .Take(100)
            .AsAsyncEnumerable();
    }
}
