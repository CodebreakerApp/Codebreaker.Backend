namespace MM.APIs.Services;

public interface IGameInitializer
{
    string[] GetColors(int holes);
}

public class RandomGameGenerator : IGameInitializer
{
    private readonly string[] Colors = { "black", "white", "red", "green", "blue", "yellow" };

    public string[] GetColors(int holes)
    {
        string[] colors = new string[holes];
        for (int i = 0; i < holes; i++)
        {
            var index = Random.Shared.Next(Colors.Length);
            colors[i] = Colors[index];
        }
        return colors;
    }
}
