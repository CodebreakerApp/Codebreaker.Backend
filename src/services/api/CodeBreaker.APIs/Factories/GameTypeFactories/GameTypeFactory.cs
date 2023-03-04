using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Factories.GameTypeFactories;

public abstract class GameTypeFactory : GameTypeFactory<string>
{
    public GameTypeFactory(string name) : base(name) { }

    public abstract override GameType Create();
}

public abstract class GameTypeFactory<TField>
{
    private readonly string _name = string.Empty;

    public GameTypeFactory(string name)
    {
        Name = name;
    }

    public string Name
    {
        get => _name;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(nameof(Name));

            _name = value;
        }
    }

    public abstract GameType<TField> Create();
}
