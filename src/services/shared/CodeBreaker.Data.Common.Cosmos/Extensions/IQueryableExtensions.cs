namespace Microsoft.EntityFrameworkCore;

public static class IQueryableExtensions
{
    public static IQueryable<T> WithPartitionKey<T>(this IQueryable<T> queryable, Guid partitionKey)
        where T : class =>
        queryable.WithPartitionKey(partitionKey.ToString());
}
