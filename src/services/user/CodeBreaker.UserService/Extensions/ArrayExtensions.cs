namespace CodeBreaker.UserService.Extensions;

internal static class ArrayExtensions
{
    public static int GetRandomIndex<TItem>(this TItem[] array) =>
        Random.Shared.Next(0, array.Length);
}
