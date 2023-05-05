using CodeBreaker.Data.ReportService.Models;

namespace CodeBreaker.Data.ReportService.Repositories;
public interface IGameRepository
{
    /// <summary>
    /// Gets all games asynchronously.
    /// </summary>
    /// <returns>All games a <see cref="IAsyncEnumerable{T}"/>.</returns>
    IAsyncEnumerable<Game> GetAsync();

    /// <summary>
    /// Gets the games with <see cref="Game.Start"/> between <paramref name="from"/> and <paramref name="to"/> asynchronously.
    /// </summary>
    /// <param name="from">The from timestamp.</param>
    /// <param name="to">The to timestamp.</param>
    /// <returns>The games with <see cref="Game.Start"/> between <paramref name="from"/> and <paramref name="to"/>.</returns>
    IAsyncEnumerable<Game> GetAsync(DateTime from, DateTime to);

    /// <summary>
    /// Gets the game asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the game.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The game.</returns>
    /// <exception cref="CodeBreaker.Data.Common.Exceptions.EntityNotFoundException">If the game with the <paramref name="id"/> was not found.</exception>
    Task<Game> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the game or default representation asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the game.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The game or null, if the game was with the <paramref name="id"/> not found.</returns>
    Task<Game?> GetOrDefaultAsync(Guid id, CancellationToken cancellationToken = default);

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
