namespace CodeBreaker.APIs.Services;

public interface IGameGenerator<out T>
{
    string TypeName { get; }
    int Holes { get; }
    T[] Colors { get; }

    T[] GetPegs();
}

/// <summary>
/// Options to define the type of the game
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Holes">number of holes</param>
/// <param name="Colors">list of color names, numbers, or forms and colors</param>
public record RandomGameGenerator<T>(string TypeName, int Holes, T[] Colors) : 
    IGameGenerator<T>
{
    public virtual T[] GetPegs()
    {
        T[] pegs = new T[Holes];
        for (int i = 0; i < Holes; i++)
        {
            var index = Random.Shared.Next(Colors.Length);
            pegs[i] = Colors[index];
        }
        return pegs;
    }
}
 
public record RandomGame6x4Generator() : 
    RandomGameGenerator<string>("6x4Game", 4, new string[] {"black", "white", "red", "green", "blue", "yellow"}),
    IGameGenerator<string>;

public record RandomGame8x5Generator() :
    RandomGameGenerator<string>("8x5Game", 5,
        new string[] {"black", "white", "red", "blue", "green", "yellow", "violet", "lightblue" });

public record RandomGame6x4MiniGenerator() :
    RandomGameGenerator<string>("6x4MiniGame", 4, new string[] { "black", "white", "red", "green", "blue", "yellow" });