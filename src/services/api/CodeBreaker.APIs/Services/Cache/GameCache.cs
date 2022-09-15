using CodeBreaker.Shared.Models.Data;
using Microsoft.Extensions.Caching.Memory;

namespace CodeBreaker.APIs.Services.Cache;

public class GameCache : InMemoryCache<Guid, Game>, IGameCache
{
    public GameCache(IMemoryCache memoryCache) : base(memoryCache)
    {
    }
}
