using System.Collections.Immutable;
using System.Reflection;

namespace CodeBreaker.APIs.Factories.GameTypeFactories;

public class GameTypeFactoryMapper<TField> : IGameTypeFactoryMapper<TField>
{
    private ImmutableDictionary<string, GameTypeFactory<TField>> _gameTypeFactories = ImmutableDictionary<string, GameTypeFactory<TField>>.Empty;

    protected virtual ImmutableDictionary<string, GameTypeFactory<TField>> GameTypes => _gameTypeFactories;

    public GameTypeFactoryMapper<TField> Initialize(IEnumerable<GameTypeFactory<TField>> gameTypeFactories)
    {
        _gameTypeFactories = gameTypeFactories
            .Select(f => new KeyValuePair<string, GameTypeFactory<TField>>(f.Name.ToUpperInvariant(), f))
            .ToImmutableDictionary();

        return this;
    }

    public GameTypeFactoryMapper<TField> Initialize(params GameTypeFactory<TField>[] gameTypeFactories) =>
        Initialize(gameTypeFactories as IEnumerable<GameTypeFactory<TField>>);

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
	public virtual GameTypeFactory<TField> GetFactoryByName(string name) =>
        _gameTypeFactories.GetValueOrDefault(name.ToUpperInvariant())
            ?? throw new GameTypeNotFoundException();

    public virtual IEnumerable<GameTypeFactory<TField>> GetAllFactories() =>
        _gameTypeFactories.Values;
}
