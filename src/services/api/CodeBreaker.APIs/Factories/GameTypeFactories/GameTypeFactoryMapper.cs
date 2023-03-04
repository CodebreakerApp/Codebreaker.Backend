using System.Collections.Immutable;
using System.Reflection;

namespace CodeBreaker.APIs.Factories.GameTypeFactories;

public class GameTypeFactoryMapper<TField> : IGameTypeFactoryMapper<TField>
{
    private ImmutableDictionary<string, GameTypeFactory<TField>> _gameTypeFactories = ImmutableDictionary<string, GameTypeFactory<TField>>.Empty;

    public GameTypeFactoryMapper<TField> Initialize() =>
        Initialize(typeof(GameTypeFactory<TField>).Assembly);

    protected virtual ImmutableDictionary<string, GameTypeFactory<TField>> GameTypes => _gameTypeFactories;

    public GameTypeFactoryMapper<TField> Initialize(Assembly assemblyToCheck)
    {
        IEnumerable<Type> gameTypeFactoryTypes =
            assemblyToCheck
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(GameTypeFactory<TField>)))
            .Where(t => !t.IsAbstract);

        _gameTypeFactories = CreateDictionaryForTypes(gameTypeFactoryTypes);
        return this;
    }

    public GameTypeFactoryMapper<TField> Initialize(IEnumerable<GameTypeFactory<TField>> gameTypeFactories)
    {
        _gameTypeFactories = gameTypeFactories
            .Select(f => new KeyValuePair<string, GameTypeFactory<TField>>(f.Name.ToUpperInvariant(), f))
            .ToImmutableDictionary();

        return this;
    }

    public GameTypeFactoryMapper<TField> Initialize(params GameTypeFactory<TField>[] gameTypeFactories) =>
        Initialize(gameTypeFactories as IEnumerable<GameTypeFactory<TField>>);

    public GameTypeFactoryMapper<TField> Initialize(IEnumerable<Type> gameTypeFactoryTypes)
    {
        if (gameTypeFactoryTypes.Any(t => !t.IsSubclassOf(typeof(GameTypeFactory<TField>)) || t.IsAbstract))
            throw new ArgumentException($"The given types must be sub-types of {nameof(GameTypeFactory)} and must not be abstract.");

        _gameTypeFactories = CreateDictionaryForTypes(gameTypeFactoryTypes);
        return this;
    }

    /// <summary>
    /// Gets the a <see cref="GameTypeFactory"/> by its name.
    /// </summary>
    /// <param name="name">The name of the requested <see cref="GameTypeFactory"/>.</param>
    /// <returns>The requested <see cref="GameTypeFactory"/>.</returns>
    /// <exception cref="GameTypeNotFoundException">Thrown when no <see cref="GameTypeFactory"/> with the given name exists.</exception>
    public virtual GameTypeFactory<TField> this[string name] =>
      GetFactoryByName(name);

    /// <summary>
    /// Gets the a <see cref="GameTypeFactory"/> by its name.
    /// </summary>
    /// <param name="name">The name of the requested <see cref="GameTypeFactory"/>.</param>
    /// <returns>The requested <see cref="GameTypeFactory"/>.</returns>
    /// <exception cref="GameTypeNotFoundException">Thrown when no <see cref="GameTypeFactory"/> with the given name exists.</exception>
    public virtual GameTypeFactory<TField> GetFactoryByName(string name)
    {
        string nameUpperCase = name.ToUpperInvariant();

        if (!_gameTypeFactories.ContainsKey(nameUpperCase))
            throw new GameTypeNotFoundException();

        return _gameTypeFactories[nameUpperCase];
    }

    public virtual IEnumerable<GameTypeFactory<TField>> GetAllFactories() =>
        _gameTypeFactories.Values;

    private ImmutableDictionary<string, GameTypeFactory<TField>> CreateDictionaryForTypes(IEnumerable<Type> gameTypeFactoryTypes) =>
        gameTypeFactoryTypes
            .Select(f =>
            {
                GameTypeFactory<TField> instance = (Activator.CreateInstance(f) as GameTypeFactory<TField>)!;
                return new KeyValuePair<string, GameTypeFactory<TField>>(instance.Name.ToUpperInvariant(), instance);

            })
            .ToImmutableDictionary();
}
