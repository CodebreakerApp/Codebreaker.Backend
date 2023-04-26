using System.Runtime.Serialization;

namespace CodeBreaker.Data.Common.Exceptions;

public class DeleteException : SaveException
{
    public DeleteException()
    {
    }

    public DeleteException(string? message) : base(message)
    {
    }

    public DeleteException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected DeleteException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
