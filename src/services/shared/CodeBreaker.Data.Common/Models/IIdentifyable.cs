namespace CodeBreaker.Data.Common.Models;

public interface IIdentifyable<T>
    where T : IEquatable<T>
{
    T Id { get; set; }
}
