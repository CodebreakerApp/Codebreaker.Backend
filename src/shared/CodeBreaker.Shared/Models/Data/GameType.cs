using System.ComponentModel;
using System.Runtime.CompilerServices;
using static CodeBreaker.Shared.Models.Data.Colors;

namespace CodeBreaker.Shared.Models.Data;

public class GameType : GameType<string>
{
    public GameType(string name, IReadOnlyList<string> fields, int holes, int maxMoves) : base(name, fields, holes, maxMoves)
    {
    }

    public static GameType Default =>
        new (
        "6x4Game",
            new string[] { Black, White, Red, Green, Blue, Yellow },
            4,
            12
        );
}

public class GameType<TField>
{
    private readonly string _name = string.Empty;

    private readonly int _holes = 0;

    private readonly int _maxMoves = 0;

    public GameType(string name, IReadOnlyList<TField> fields, int holes, int maxMoves)
    {
        Name = name;
        Fields = fields;
        Holes = holes;
        MaxMoves = maxMoves;
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

    public IReadOnlyList<TField> Fields { get; init; } = new List<TField>();

    public int Holes
    {
        get => _holes;
        init
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(Holes));

            _holes = value;
        }
    }

    public int MaxMoves
    {
        get => _maxMoves;
        init
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(MaxMoves));

            _maxMoves = value;
        }
    }

    public override string ToString() => Name;
}








public interface IField : IFieldVisitable { }

public class ColorField : IField
{
    public string Color { get; set; } = string.Empty;

    public TResult Accept<TResult>(IFieldVisitor<TResult> visitor) =>
        visitor.Visit(this);

    public void Accept(IFieldVisitor visitor) =>
        visitor.Visit(this);
}

public class ColorShapeField : ColorField
{
    public string Shape { get; set; } = string.Empty;
}



public class PrintFieldVisitor : IFieldVisitor
{
    public void Visit(ColorField field) => throw new NotImplementedException();

    public void Visit(ColorShapeField field) => throw new NotImplementedException();
}



/// <summary>
/// Interface to implement for classes visiting others. 
/// See Visitor design pattern for more details.
/// </summary>
/// <typeparam name="TVisited">The type of the visited.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
//public interface IVisitor<TVisited, TResult> : IVisitor where TVisited : IVisitable
//{
//    TResult Visit(TVisited visited);
//}

/// <summary>
/// Marking interface.
/// </summary>
public interface IVisitor { }

//public interface IFieldVisitor<TResult> : IVisitor
//{
//    TResult Visit(IFieldVisitable visitable);
//}

// OR

public interface IFieldVisitor<TResult> : IVisitor
{
    TResult Visit(ColorField field);

    TResult Visit(ColorShapeField field);
}

public interface IFieldVisitor : IVisitor
{
    void Visit(ColorField field);

    void Visit(ColorShapeField field);
}

//public interface IGameTypeVisitor<TResult> : IVisitor<IGameTypeVisitable, TResult> { }

/// <summary>
/// Interface to implement for classes visitable by a visitor.
/// See Visitor design pattern for more details.
/// </summary>
/// <typeparam name="TVisitor">The type of the visitor.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IVisitable<TVisitor, TResult> : IVisitable where TVisitor : IVisitor
{
    TResult Accept(TVisitor visitor);
}

public interface IFieldVisitable : IVisitable
{
    void Accept(IFieldVisitor visitor);

    TResult Accept<TResult>(IFieldVisitor<TResult> visitor);
}

//public interface IGameTypeVisitable : IVisitable
//{
//    TResult Accept<TResult>(IGameTypeVisitor<TResult> visitor) =>
//        visitor.Visit(this);
//}

/// <summary>
/// Marking interface.
/// </summary>
public interface IVisitable { }






// Daran denken, dass im Visitor auch ein State gespeichert werden kann. =Alternative zum Rückgabewert
