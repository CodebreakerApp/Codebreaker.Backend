using Codebreaker.GameAPIs.Extensions;
using Codebreaker.GameAPIs.Models;

namespace Codebreaker.GameAPIs.Algorithms.Tests;

public class MockColorGame : ICalculatableGame<ColorField, ColorResult>
{
    public int Holes { get; init; }
    public int MaxMoves { get; init; }
    public DateTime EndTime { get; set; }
    public bool Won { get; set; }

    public required IEnumerable<ColorField> Fields { get; init; }
    public required ICollection<ColorField> Codes { get; init; }

    private List<ICalculatableMove<ColorField, ColorResult>> _moves = new();
    public ICollection<ICalculatableMove<ColorField, ColorResult>> Moves => _moves;
}

public class MockColorMove : ICalculatableMove<ColorField, ColorResult>
{
    public int MoveNumber { get; set; }
    public required ICollection<ColorField> GuessPegs { get; init; }
    public ColorResult KeyPegs { get; set; }
}
