using System.Runtime.Serialization;

namespace CodeBreaker.APIs.Exceptions;

public class EventBatchSizeException : ApplicationException
{
    public EventBatchSizeException()
    {
    }

    public EventBatchSizeException(string? message) : base(message)
    {
    }

    public EventBatchSizeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected EventBatchSizeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
