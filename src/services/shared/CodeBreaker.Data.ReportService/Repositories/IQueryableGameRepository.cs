using CodeBreaker.Data.ReportService.Models;

namespace CodeBreaker.Data.ReportService.Repositories;
public interface IQueryableGameRepository
{
    /// <summary>
    /// Gets the queryable games.
    /// </summary>
    /// <value>
    /// The queryable games.
    /// </value>
    IQueryable<Game> QueryableGames { get; }

    /// <summary>
    /// Creates the game asynchronously.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="CodeBreaker.Data.Common.Exceptions.CreateException">
    /// Could not create the game due to an concurrency error in the database
    /// or
    /// Could not create the game
    /// </exception>
    Task CreateAsync(Game game, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the game asynchronously.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="CodeBreaker.Data.Common.Exceptions.UpdateException">
    /// Could not update the game due to an concurrency error in the database
    /// or
    /// Could not update the game
    /// </exception>
    Task UpdateAsync(Game entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the game asynchronously.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="CodeBreaker.Data.Common.Exceptions.UpdateException">
    /// Could not delete the game due to an concurrency error in the database
    /// or
    /// Could not delete the game
    /// </exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
