using CodeBreaker.Data.Common;
using CodeBreaker.Data.Common.Exceptions;
using CodeBreaker.Data.ReportService.DbContexts;
using CodeBreaker.Data.ReportService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeBreaker.Data.ReportService.Repositories;

public class GameRepository : IQueryableGameRepository, IGameRepository
{
    private readonly ILogger _logger;

    private readonly GameContext _gameContext;

    public const string EntityName = nameof(Game);

    public GameRepository(GameContext gameContext, ILogger<GameRepository> logger)
    {
        _logger = logger;
        _gameContext = gameContext;
    }

    protected DbSet<Game> Games => _gameContext.Set<Game>();

    public IQueryable<Game> QueryableGames => Games;

    public IAsyncEnumerable<Game> GetAsync() =>
        Games.AsAsyncEnumerable();

    public IAsyncEnumerable<Game> GetAsync(DateTime from, DateTime to) =>
        Games
            .Where(x => x.Start >= from && x.Start <= to)
            .AsAsyncEnumerable();

    public async Task<Game?> GetOrDefaultAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Games
            .WithPartitionKey(id)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Game> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await GetOrDefaultAsync(id, cancellationToken) ?? throw new EntityNotFoundException(EntityName, id.ToString());

    public async Task CreateAsync(Game game, CancellationToken cancellationToken = default)
    {
        Games.Add(game);

        try
        {
            await _gameContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Error while creating the game");
            throw new RepositoryException("Could not create the game due to an concurrency error in the database", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error while creating the game");
            throw new RepositoryException("Could not create the game", ex);
        }

        _logger.EntityCreated(EntityName, game.Id.ToString());
    }

    public async Task UpdateAsync(Game game, CancellationToken cancellationToken = default)
    {
        Games.Update(game);
     
        try
        {
            await _gameContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Error while updating the game with the id {id}", game.Id);
            throw new RepositoryException("Could not update the game due to an concurrency error in the database", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error while updating the game with the id {id}", game.Id);
            throw new RepositoryException("Could not update the game", ex);
        }

        _logger.EntityUpdated(EntityName, game.Id.ToString()!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetAsync(id, cancellationToken);
        Games.Remove(entity);

        try
        {
            await _gameContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Error while deleting the game with the id {id}", id);
            throw new RepositoryException("Could not delete the game due to an concurrency error in the database", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error while deleting the game with the id {id}", id);
            throw new RepositoryException("Could not delete the game", ex);
        }

        _logger.EntityDeleted(EntityName, entity.Id.ToString()!);
    }
}
