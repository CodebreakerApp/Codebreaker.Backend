using System.Diagnostics.CodeAnalysis;

namespace CodeBreaker.APIs.Services.Cache
{
    public interface IApplicationCache<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : class
    {
        TValue Get(TKey key);

        TValue? GetOrDefault(TKey key);

        TValue? GetOrSet(TKey key, TValue value);

        void Remove(TKey key);

        void Set(TKey key, TValue value);

        bool TryGet(TKey key, [NotNullWhen(returnValue: true)] out TValue? result);
    }
}