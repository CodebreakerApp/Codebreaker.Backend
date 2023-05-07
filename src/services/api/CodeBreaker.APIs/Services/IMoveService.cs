using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Services;

public interface IMoveService
{
    Task<Game> CreateMoveAsync(Guid gameId, Move move);
}