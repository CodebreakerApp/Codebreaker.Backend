using Codebreaker.GameAPIs.Extensions;
using Codebreaker.GameAPIs.Models;

namespace Codebreaker.GameAPIs.Algorithms.Tests;

public class MockShapeGame : ICalculatableGame<ShapeAndColorField, ShapeAndColorResult>
{
    public int Holes { get; init; }
    public int MaxMoves { get; init; }
    public DateTime EndTime { get; set; }
    public bool Won { get; set; }

    public required IEnumerable<ShapeAndColorField> Fields { get; init; }
    public required ICollection<ShapeAndColorField> Codes { get; init; }

    private List<ICalculatableMove<ShapeAndColorField, ShapeAndColorResult>> _moves = new();
    public ICollection<ICalculatableMove<ShapeAndColorField, ShapeAndColorResult>> Moves => _moves;
}

public class MockShapeMove : ICalculatableMove<ShapeAndColorField, ShapeAndColorResult>
{
    public int MoveNumber { get; set; }
    public required ICollection<ShapeAndColorField> GuessPegs { get; init; }
    public ShapeAndColorResult KeyPegs { get; set; }
}
