using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Codebreaker.GameAPIs.Client.Models;

namespace CodeBreaker.Bot.Benchmarks;

/// <summary>
/// Benchmarks that simulate realistic game scenarios
/// These test combinations of operations as they would occur during actual gameplay
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class GameScenarioBenchmarks
{
    private List<int> _initialValues6x4 = null!;
    private List<int> _initialValues8x5 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _initialValues6x4 = BenchmarkTestData.CreateGame6x4PossibleValues();
        _initialValues8x5 = BenchmarkTestData.CreateGame8x5PossibleValues();
    }

    #region Typical Game Progression Scenarios

    [Benchmark]
    [BenchmarkCategory("GameScenario", "Game6x4", "EarlyGame")]
    public List<int> SimulateEarlyGame6x4_Move1()
    {
        // Simulate first move with no matches
        var selection = BenchmarkTestData.CreateTestSelection(GameType.Game6x4);
        return _initialValues6x4.HandleNoMatches(GameType.Game6x4, selection);
    }

    [Benchmark]
    [BenchmarkCategory("GameScenario", "Game6x4", "MidGame")]
    public List<int> SimulateMidGame6x4_Move3()
    {
        // Simulate mid-game with some black matches
        var values = _initialValues6x4.ToList();
        var selection1 = BenchmarkTestData.CreateTestSelection(GameType.Game6x4);

        // First move: no matches
        values = values.HandleNoMatches(GameType.Game6x4, selection1);

        // Second move: 1 black match
        var selection2 = 0b_001000_000100_000100_000100; // Different first position
        values = values.HandleBlackMatches(GameType.Game6x4, 1, selection2);

        // Third move: 2 white matches
        var selection3 = 0b_000100_001000_000100_000100; // Rearranged colors
        return values.HandleWhiteMatches(GameType.Game6x4, 2, selection3);
    }

    [Benchmark]
    [BenchmarkCategory("GameScenario", "Game6x4", "LateGame")]
    public List<int> SimulateLateGame6x4_Move5()
    {
        // Simulate late game with high precision
        var values = _initialValues6x4.ToList();
        var baseSelection = BenchmarkTestData.CreateTestSelection(GameType.Game6x4);

        // Apply multiple filtering operations
        values = values.HandleNoMatches(GameType.Game6x4, baseSelection);
        values = values.HandleBlackMatches(GameType.Game6x4, 2, 0b_001000_000100_000100_000100);
        values = values.HandleWhiteMatches(GameType.Game6x4, 3, 0b_000100_001000_010000_000100);

        // Final precise move
        return values.HandleBlackMatches(GameType.Game6x4, 3, 0b_001000_010000_000100_100000);
    }

    [Benchmark]
    [BenchmarkCategory("GameScenario", "Game8x5", "EarlyGame")]
    public List<int> SimulateEarlyGame8x5_Move1()
    {
        // Simulate first move for 8x5 game
        var selection = BenchmarkTestData.CreateTestSelection(GameType.Game8x5);
        return _initialValues8x5.HandleNoMatches(GameType.Game8x5, selection);
    }

    [Benchmark]
    [BenchmarkCategory("GameScenario", "Game8x5", "MidGame")]
    public List<int> SimulateMidGame8x5_Move3()
    {
        // Simulate more complex 8x5 game progression
        var values = _initialValues8x5.ToList();

        // Move 1: Some white matches
        values = values.HandleWhiteMatches(GameType.Game8x5, 3, 0b_000100_001000_010000_100000_000010);

        // Move 2: Black matches
        values = values.HandleBlackMatches(GameType.Game8x5, 2, 0b_001000_000100_010000_000010_100000);

        // Move 3: More precise filtering
        return values.HandleBlackMatches(GameType.Game8x5, 4, 0b_000100_001000_000010_010000_100000);
    }

    #endregion

    #region Memory-Intensive Scenarios

    [Benchmark]
    [BenchmarkCategory("GameScenario", "Memory", "WorstCase")]
    public List<int> SimulateWorstCaseFiltering()
    {
        // Scenario where filtering operations don't reduce the list much
        var values = _initialValues6x4.ToList();

        // Multiple operations that don't filter much
        for (int i = 0; i < 5; i++)
        {
            var selection = 0b_000001_000001_000001_000001 << i; // Different selections
            values = values.HandleWhiteMatches(GameType.Game6x4, 1, selection);
        }

        return values;
    }

    [Benchmark]
    [BenchmarkCategory("GameScenario", "Memory", "BestCase")]
    public List<int> SimulateBestCaseFiltering()
    {
        // Scenario where filtering operations reduce the list significantly
        var values = _initialValues6x4.ToList();

        // Operations that should filter aggressively
        values = values.HandleBlackMatches(GameType.Game6x4, 3, BenchmarkTestData.CreateTestSelection(GameType.Game6x4));
        values = values.HandleBlackMatches(GameType.Game6x4, 3, 0b_001000_010000_100000_000010);

        return values;
    }

    #endregion

    #region Combined Operations Benchmark

    [Benchmark]
    [BenchmarkCategory("GameScenario", "Combined", "FullGameSimulation")]
    public int SimulateCompleteGame6x4()
    {
        var values = _initialValues6x4.ToList();
        int moveCount = 0;

        // Simulate a complete game until very few values remain
        while (values.Count > 10 && moveCount < 8)
        {
            moveCount++;
            var selection = 0b_000100_000100_000100_000100 << (moveCount % 6);

            switch (moveCount % 4)
            {
                case 0:
                    values = values.HandleNoMatches(GameType.Game6x4, selection);
                    break;
                case 1:
                    if (values.Count > 100)
                        values = values.HandleBlackMatches(GameType.Game6x4, 1, selection);
                    break;
                case 2:
                    if (values.Count > 50)
                        values = values.HandleWhiteMatches(GameType.Game6x4, 2, selection);
                    break;
                case 3:
                    if (values.Count > 25)
                        values = values.HandleBlackMatches(GameType.Game6x4, 2, selection);
                    break;
            }
        }

        return values.Count;
    }

    #endregion
}