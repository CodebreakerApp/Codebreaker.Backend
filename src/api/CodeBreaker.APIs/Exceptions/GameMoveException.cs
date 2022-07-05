namespace CodeBreaker.APIs.Exceptions;

[Serializable]
public class GameMoveException : Exception
{
    public GameMoveException() { }
    public GameMoveException(string message) : base(message) { }
    public GameMoveException(string message, Exception inner) : base(message, inner) { }
    protected GameMoveException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
