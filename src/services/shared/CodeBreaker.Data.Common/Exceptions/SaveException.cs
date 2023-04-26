using System.Runtime.Serialization;

namespace CodeBreaker.Data.Common.Exceptions;

public class SaveException : Exception
{
    public SaveException()
    {
    }

    public SaveException(string? message) : base(message)
    {
    }

    public SaveException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SaveException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
