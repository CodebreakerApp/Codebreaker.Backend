namespace CodeBreaker.APIs.Services;

public interface IGameInitializer<out T>
{
    T[] GetColors();
}

public class Random6x4GameGenerator : IGameInitializer<string>
{
    private const int holes = 4;
    private readonly string[] Colors = { "black", "white", "red", "green", "blue", "yellow" };

    public string[] GetColors()
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
