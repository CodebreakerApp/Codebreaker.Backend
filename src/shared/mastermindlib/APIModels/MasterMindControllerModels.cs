namespace MM.Shared.APIModels;

public record struct CreateGameRequest(string Name);

public record struct CreateGameResponse(string Id);

public record struct MoveRequest(string Id, int MoveNumber, IEnumerable<string> CodePegs)
{
    public override string ToString() => $"{Id}, {MoveNumber}. {string.Join("..", CodePegs)}";
}

public record struct MoveResponse(string Id, bool Completed = false, bool Won = false, IEnumerable<string>? KeyPegs = null)
{
    public override string ToString()
    {
        string completedText = Completed ? "finished" : string.Empty;
        string wonText = Won ? "I won" : string.Empty;
        string keyPegText = KeyPegs switch
        {
            null => "nothing",
            _ => string.Join("..", KeyPegs)
        };
        return $"{Id}, {completedText} {wonText} {keyPegText}";
    }
}
