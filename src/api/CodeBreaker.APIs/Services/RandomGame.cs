using static CodeBreaker.Shared.CodeBreakerColors;

namespace CodeBreaker.APIs.Services;

public interface IGame<out T>
{
    /// Type of the game as defined by <see cref="GameTypes" />
    string TypeName { get; }
    /// <summary>
    /// Number of holes
    /// </summary>
    int Holes { get; }
    /// <summary>
    /// The list of possible colors
    /// </summary>
    T[] Colors { get; }

    /// <summary>
    /// Get the code for the game
    /// </summary>
    /// <returns>the correct code for the game</returns>
    T[] GetCode();
}

/// <summary>
/// Options to define the type of the game
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Holes">number of holes</param>
/// <param name="Colors">list of color names, numbers, or forms and colors</param>
public record RandomGame<T>(string TypeName, int Holes, T[] Colors) : 
    IGame<T>
{
    public virtual T[] GetCode()
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
 
public record RandomGame6x4() : 
    RandomGame<string>("6x4Game", 4, new string[] { Black, White, Red, Green, Blue, Yellow }),
    IGame<string>;

public record RandomGame8x5() :
    RandomGame<string>("8x5Game", 5,
        new string[] { Black, White, Red, Blue, Green, Yellow, Violet, LightBlue });

public record RandomGame6x4Mini() :
    RandomGame<string>("6x4MiniGame", 4, new string[] { Black, White, Red, Green, Blue, Yellow });