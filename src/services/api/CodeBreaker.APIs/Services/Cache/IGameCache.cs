using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Services.Cache;

public interface IGameCache : IApplicationCache<Guid, Game>
{
}