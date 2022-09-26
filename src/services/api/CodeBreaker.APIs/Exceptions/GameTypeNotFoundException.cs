namespace CodeBreaker.APIs.Exceptions;

public class GameTypeNotFoundException : NotFoundException
{
    public GameTypeNotFoundException()
    {
    }

    public GameTypeNotFoundException(string? message) : base(message)
    {
    }

    public GameTypeNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
