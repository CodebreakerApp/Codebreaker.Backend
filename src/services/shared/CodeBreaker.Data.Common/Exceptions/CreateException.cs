using System.Runtime.Serialization;

namespace CodeBreaker.Data.Common.Exceptions;

public class CreateException : SaveException
{
    public CreateException()
    {
    }

    public CreateException(string? message) : base(message)
    {
    }

    public CreateException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CreateException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
