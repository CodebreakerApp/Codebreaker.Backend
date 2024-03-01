namespace Codebreaker.GameAPIs.Models.Tests;

public class GameTests
{
    [Fact]
    public void CreatedGame_Should_Serialize_And_Deserialize()
    {
        Game game = CreateGameSkeleton();
        string json = JsonSerializer.Serialize(game);
        Game? actualGame = JsonSerializer.Deserialize<Game>(json);
        Assert.Equivalent(game, actualGame);
    }

    [Fact]
    public void GameWithMove_Should_Serialize_And_Deserialize()
    {
        Game game = CreateGameWithMoveSkeleton();
        string json = JsonSerializer.Serialize(game);
        Game? actualGame = JsonSerializer.Deserialize<Game>(json);
        Assert.Equivalent(game, actualGame);
    }

    [Fact]
    public void GameWithMove_Should_Serialize_And_Deserialize_Bytes()
    {
        Game game = CreateGameWithMoveSkeleton();
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(game);
        Game? actualGame = JsonSerializer.Deserialize<Game>(data);
        Assert.Equivalent(game, actualGame);
    }

    private static Game CreateGameSkeleton()
    {
        Game game = new(Guid.NewGuid(), "Game6x4", "test", DateTime.Now, 4, 12)
        {
            Codes = ["Red", "Green", "Yellow", "Blue", "Orange", "Purple"],
            FieldValues = new Dictionary<string, IEnumerable<string>>
            {
                ["colors"] = ["Red", "Green", "Yellow", "Blue"]
            }
        };
        return game;
    }

    private static Game CreateGameWithMoveSkeleton()
    {
        Game game = CreateGameSkeleton();

        Move move = new(Guid.NewGuid(), 1)
        {
            GuessPegs = ["Red", "Red", "Red", "Red"],
            KeyPegs = ["Black"]
        };
        game.LastMoveNumber = 1;
        game.Moves.Add(move);
        return game;
    }
}
