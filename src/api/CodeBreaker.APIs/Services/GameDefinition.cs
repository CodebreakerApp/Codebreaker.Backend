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
    T[] CreateRandomCode();
}

/// <summary>
/// Options to define the type of the game
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Holes">number of holes</param>
/// <param name="Colors">list of color names, numbers, or forms and colors</param>
public record GameDefinition<T>(string TypeName, int Holes, int MaxMoves, T[] Colors) : 
    IGame<T>
{
    public virtual T[] CreateRandomCode()
    {
        T[] pegs = new T[Holes];
        for (int i = 0; i < Holes; i++)
        {
            var index = Random.Shared.Next(Colors.Length);
            pegs[i] = Colors[index];
        }
        return pegs;
    }

    /// <summary>
    /// Get a concrete GameXXXDefinition based on a string.
    /// Use this method only if the game definition is needed based on a string.
    /// If you know the definition you need, use the constructor of the concrete type.
    /// </summary>
    /// <param name="TypeName">The string specified by the game defintion</param>
    /// <returns>A concrete GameDefinition derived type</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static GameDefinition<string> GetGameDefinition(string TypeName) =>
        TypeName switch
        {
            "6x4Game" => new Game6x4Definition(),
            "8x5Game" => new Game8x5Definition(),
            "6x4MiniGame" => new Game6x4MiniDefinitition(),
            _ => throw new InvalidOperationException("not yet supported")
        };
}
 
public record Game6x4Definition() : 
    GameDefinition<string>("6x4Game", Holes: 4, MaxMoves: 12, 
        new string[] { Black, White, Red, Green, Blue, Yellow }),
    IGame<string>;

public record Game8x5Definition() :
    GameDefinition<string>("8x5Game", Holes: 5, MaxMoves: 12,
        new string[] { Black, White, Red, Blue, Green, Yellow, Violet, LightBlue }),
    IGame<string>;

public record Game6x4MiniDefinitition() :
    GameDefinition<string>("6x4MiniGame", Holes: 4, MaxMoves: 12,
        new string[] { Black, White, Red, Green, Blue, Yellow }),
    IGame<string>;