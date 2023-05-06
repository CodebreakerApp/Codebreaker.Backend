using System.Reflection;

namespace CodeBreaker.APIs.Factories.GameTypeFactories;

public interface IGameTypeFactoryMapper : IGameTypeFactoryMapper<string> { }

public interface IGameTypeFactoryMapper<TField>
{
    GameTypeFactory<TField> this[string name] { get; }

    GameTypeFactory<TField> GetFactoryByName(string name);

    IEnumerable<GameTypeFactory<TField>> GetAllFactories();

    GameTypeFactoryMapper<TField> Initialize(IEnumerable<GameTypeFactory<TField>> gameTypeFactories);
}
