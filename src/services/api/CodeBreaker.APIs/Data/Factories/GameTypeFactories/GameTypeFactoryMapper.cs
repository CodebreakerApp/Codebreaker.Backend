using System.Collections.Immutable;
using System.Reflection;

namespace CodeBreaker.APIs.Data.Factories.GameTypeFactories;

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

	public GameTypeFactoryMapper<TField> Initialize(IEnumerable<Type> gameTypeFactoryTypes)
	{
		if (gameTypeFactoryTypes.Any(t => !t.IsSubclassOf(typeof(GameTypeFactory<TField>)) || t.IsAbstract))
			throw new ArgumentException($"The given types must be sub-types of {nameof(GameTypeFactory)} and must not be abstract.");

		_gameTypeFactories = CreateDictionaryForTypes(gameTypeFactoryTypes);
		return this;
	}

	public virtual GameTypeFactory<TField> this[string name] =>
		_gameTypeFactories[name.ToUpperInvariant()];

	public virtual GameTypeFactory<TField> GetFactoryByName(string name) =>
		_gameTypeFactories[name.ToUpperInvariant()];

	private ImmutableDictionary<string, GameTypeFactory<TField>> CreateDictionaryForTypes(IEnumerable<Type> gameTypeFactoryTypes) =>
		gameTypeFactoryTypes
			.Select(f =>
			{
				GameTypeFactory<TField> instance = (Activator.CreateInstance(f) as GameTypeFactory<TField>)!;
				return new KeyValuePair<string, GameTypeFactory<TField>>(instance.Name.ToUpperInvariant(), instance);

            })
			.ToImmutableDictionary();
}
