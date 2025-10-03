using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace CodeBreaker.Bot.Benchmarks;

/// <summary>
/// Benchmarks for the core CodeBreaker algorithm methods
/// Measures execution time and memory consumption for filtering and matching operations
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class AlgorithmBenchmarks
{
    private List<int> _fullGame6x4Values = null!;
    private List<int> _fullGame8x5Values = null!;
    private List<int> _reducedGame6x4Values = null!;
    private List<int> _reducedGame8x5Values = null!;
    private List<int> _smallGame6x4Values = null!;

    private int _testSelection6x4;
    private int _testSelection8x5;
    private int _testSelection5x5x4;

    private Dictionary<int, string> _colorNames6x4 = null!;
    private Dictionary<int, string> _colorNames8x5 = null!;
    private Dictionary<int, string> _colorNames5x5x4 = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize full possible values for different game types
        _fullGame6x4Values = BenchmarkTestData.CreateGame6x4PossibleValues();
        _fullGame8x5Values = BenchmarkTestData.CreateGame8x5PossibleValues();

        // Create reduced lists simulating games in progress
        _reducedGame6x4Values = BenchmarkTestData.CreateReducedPossibleValues(_fullGame6x4Values, 100);
        _reducedGame8x5Values = BenchmarkTestData.CreateReducedPossibleValues(_fullGame8x5Values, 200);
        _smallGame6x4Values = BenchmarkTestData.CreateReducedPossibleValues(_fullGame6x4Values, 20);

        // Create test selections
        _testSelection6x4 = BenchmarkTestData.CreateTestSelection(GameType.Game6x4);
        _testSelection8x5 = BenchmarkTestData.CreateTestSelection(GameType.Game8x5);
        _testSelection5x5x4 = BenchmarkTestData.CreateTestSelection(GameType.Game5x5x4);

        // Create color name mappings
        _colorNames6x4 = BenchmarkTestData.CreateColorNames(GameType.Game6x4);
        _colorNames8x5 = BenchmarkTestData.CreateColorNames(GameType.Game8x5);
        _colorNames5x5x4 = BenchmarkTestData.CreateColorNames(GameType.Game5x5x4);
    }

    #region Black Matches Benchmarks

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game6x4", "FullList")]
    public List<int> HandleBlackMatches_Game6x4_FullList()
    {
        return BinaryCodeBreakerAlgorithms.HandleBlackMatches(_fullGame6x4Values, GameType.Game6x4, 2, _testSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game6x4", "ReducedList")]
    public List<int> HandleBlackMatches_Game6x4_ReducedList()
    {
        return _reducedGame6x4Values.HandleBlackMatches(GameType.Game6x4, 2, _testSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game6x4", "SmallList")]
    public List<int> HandleBlackMatches_Game6x4_SmallList()
    {
        return _smallGame6x4Values.HandleBlackMatches(GameType.Game6x4, 1, _testSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game8x5", "FullList")]
    public List<int> HandleBlackMatches_Game8x5_FullList()
    {
        return _fullGame8x5Values.HandleBlackMatches(GameType.Game8x5, 3, _testSelection8x5);
    }

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game8x5", "ReducedList")]
    public List<int> HandleBlackMatches_Game8x5_ReducedList()
    {
        return _reducedGame8x5Values.HandleBlackMatches(GameType.Game8x5, 2, _testSelection8x5);
    }

    #endregion

    #region White Matches Benchmarks

    [Benchmark]
    [BenchmarkCategory("WhiteMatches", "Game6x4", "FullList")]
    public List<int> HandleWhiteMatches_Game6x4_FullList()
    {
        return _fullGame6x4Values.HandleWhiteMatches(GameType.Game6x4, 3, _testSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("WhiteMatches", "Game6x4", "ReducedList")]
    public List<int> HandleWhiteMatches_Game6x4_ReducedList()
    {
        return _reducedGame6x4Values.HandleWhiteMatches(GameType.Game6x4, 2, _testSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("WhiteMatches", "Game8x5", "ReducedList")]
    public List<int> HandleWhiteMatches_Game8x5_ReducedList()
    {
        return _reducedGame8x5Values.HandleWhiteMatches(GameType.Game8x5, 4, _testSelection8x5);
    }

    #endregion

    #region Blue Matches Benchmarks

    [Benchmark]
    [BenchmarkCategory("BlueMatches", "Game5x5x4", "ReducedList")]
    public List<int> HandleBlueMatches_Game5x5x4_ReducedList()
    {
        return _reducedGame6x4Values.HandleBlueMatches(GameType.Game5x5x4, 2, _testSelection5x5x4);
    }

    [Benchmark]
    [BenchmarkCategory("BlueMatches", "Game6x4", "ReducedList")]
    public List<int> HandleBlueMatches_Game6x4_ReducedList()
    {
        // For non-Game5x5x4, this should return the list unchanged
        return _reducedGame6x4Values.HandleBlueMatches(GameType.Game6x4, 2, _testSelection6x4);
    }

    #endregion

    #region No Matches Benchmarks

    [Benchmark]
    [BenchmarkCategory("NoMatches", "Game6x4", "FullList")]
    public List<int> HandleNoMatches_Game6x4_FullList()
    {
        return _fullGame6x4Values.HandleNoMatches(GameType.Game6x4, _testSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("NoMatches", "Game6x4", "ReducedList")]
    public List<int> HandleNoMatches_Game6x4_ReducedList()
    {
        return _reducedGame6x4Values.HandleNoMatches(GameType.Game6x4, _testSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("NoMatches", "Game8x5", "ReducedList")]
    public List<int> HandleNoMatches_Game8x5_ReducedList()
    {
        return _reducedGame8x5Values.HandleNoMatches(GameType.Game8x5, _testSelection8x5);
    }

    #endregion

    #region Peg Selection Benchmarks

    [Benchmark]
    [BenchmarkCategory("PegSelection", "Game6x4")]
    public int SelectPeg_Game6x4_Position0()
    {
        return _testSelection6x4.SelectPeg(GameType.Game6x4, 0);
    }

    [Benchmark]
    [BenchmarkCategory("PegSelection", "Game6x4")]
    public int SelectPeg_Game6x4_Position3()
    {
        return _testSelection6x4.SelectPeg(GameType.Game6x4, 3);
    }

    [Benchmark]
    [BenchmarkCategory("PegSelection", "Game8x5")]
    public int SelectPeg_Game8x5_Position0()
    {
        return _testSelection8x5.SelectPeg(GameType.Game8x5, 0);
    }

    [Benchmark]
    [BenchmarkCategory("PegSelection", "Game8x5")]
    public int SelectPeg_Game8x5_Position4()
    {
        return _testSelection8x5.SelectPeg(GameType.Game8x5, 4);
    }

    #endregion

    #region Color Conversion Benchmarks

    [Benchmark]
    [BenchmarkCategory("ColorConversion", "Game6x4")]
    public string[] IntToColors_Game6x4()
    {
        return _testSelection6x4.IntToColors(GameType.Game6x4, _colorNames6x4);
    }

    [Benchmark]
    [BenchmarkCategory("ColorConversion", "Game8x5")]
    public string[] IntToColors_Game8x5()
    {
        return _testSelection8x5.IntToColors(GameType.Game8x5, _colorNames8x5);
    }

    [Benchmark]
    [BenchmarkCategory("ColorConversion", "Game5x5x4")]
    public string[] IntToColors_Game5x5x4()
    {
        return _testSelection5x5x4.IntToColors(GameType.Game5x5x4, _colorNames5x5x4);
    }

    #endregion
}