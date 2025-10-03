using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace CodeBreaker.Bot.Benchmarks;

/// <summary>
/// Benchmarks for the initialization methods that create possible values lists
/// These are typically called once per game but can be memory-intensive
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class InitializationBenchmarks
{
    #region Possible Values Initialization

    [Benchmark]
    [BenchmarkCategory("Initialization", "Game6x4")]
    public List<int> InitializePossibleValues_Game6x4()
    {
        return BenchmarkTestData.CreateGame6x4PossibleValues();
    }

    [Benchmark]
    [BenchmarkCategory("Initialization", "Game8x5")]
    public List<int> InitializePossibleValues_Game8x5()
    {
        return BenchmarkTestData.CreateGame8x5PossibleValues();
    }

    #endregion

    #region List Operations

    [Benchmark]
    [BenchmarkCategory("ListOperations", "Memory")]
    public List<int> CreateAndSortLargeList()
    {
        var values = BenchmarkTestData.CreateGame6x4PossibleValues();
        values.Sort();
        return values;
    }

    [Benchmark]
    [BenchmarkCategory("ListOperations", "Memory")]
    public List<int> CreateReducedList()
    {
        var fullList = BenchmarkTestData.CreateGame6x4PossibleValues();
        return BenchmarkTestData.CreateReducedPossibleValues(fullList, 100);
    }

    #endregion

    #region Memory Intensive Operations

    [Benchmark]
    [BenchmarkCategory("Memory", "Large")]
    public int CountGame6x4Values()
    {
        var values = BenchmarkTestData.CreateGame6x4PossibleValues();
        return values.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Memory", "Large")]
    public int CountGame8x5Values()
    {
        var values = BenchmarkTestData.CreateGame8x5PossibleValues();
        return values.Count;
    }

    #endregion
}