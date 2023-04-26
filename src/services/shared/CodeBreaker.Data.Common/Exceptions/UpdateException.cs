using System.Runtime.Serialization;

namespace CodeBreaker.Data.Common.Exceptions;

public class UpdateException : SaveException
{
    public UpdateException()
    {
    }

    public UpdateException(string? message) : base(message)
    {
    }

    public UpdateException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected UpdateException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
