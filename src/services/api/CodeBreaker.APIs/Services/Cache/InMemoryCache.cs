using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;

namespace CodeBreaker.APIs.Services.Cache;

public class InMemoryCache<TKey, TValue> : IApplicationCache<TKey, TValue>
	where TKey : notnull, IEquatable<TKey>
	where TValue : class
{
	private readonly IMemoryCache _memoryCache;

	public InMemoryCache(IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
	}

	public void Set(TKey key, TValue value) =>
		_memoryCache.Set(key, value);

	public bool TryGet(TKey key, [NotNullWhen(returnValue: true)] out TValue? result) =>
		_memoryCache.TryGetValue(key, out result);

	public TValue Get(TKey key) =>
		_memoryCache.Get(key) as TValue ?? throw new KeyNotFoundException($"The key '{key}' was not found in the cache.");

	public TValue? GetOrDefault(TKey key) =>
		_memoryCache.Get(key) as TValue;

	public TValue? GetOrSet(TKey key, TValue value) =>
		_memoryCache.GetOrCreate(key, x => x.SetValue(value))?.Value as TValue;

	public void Remove(TKey key) =>
		_memoryCache.Remove(key);
}
