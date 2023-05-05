using System.Runtime.Serialization;

namespace CodeBreaker.Queuing.Common.Exceptions;

public class NoItemException : Exception
{
    public NoItemException()
    {
    }

    public NoItemException(string? message) : base(message)
    {
    }

    public NoItemException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected NoItemException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
