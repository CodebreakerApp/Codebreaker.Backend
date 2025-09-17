using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace CodeBreaker.Bot.Benchmarks;

/// <summary>
/// Comprehensive benchmarks comparing binary vs string-based CodeBreaker algorithms
/// Tests performance and memory consumption of both implementations across different scenarios
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class ComparisonBenchmarks
{
    // Binary algorithm test data
    private List<int> _binaryFullGame6x4Values = null!;
    private List<int> _binaryReducedGame6x4Values = null!;
    private List<int> _binaryFullGame8x5Values = null!;
    private List<int> _binaryReducedGame8x5Values = null!;
    private int _binaryTestSelection6x4;
    private int _binaryTestSelection8x5;
    private Dictionary<int, string> _colorNames6x4 = null!;
    private Dictionary<int, string> _colorNames8x5 = null!;

    // String algorithm test data
    private List<string[]> _stringFullGame6x4Values = null!;
    private List<string[]> _stringReducedGame6x4Values = null!;
    private List<string[]> _stringFullGame8x5Values = null!;
    private List<string[]> _stringReducedGame8x5Values = null!;
    private string[] _stringTestSelection6x4 = null!;
    private string[] _stringTestSelection8x5 = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize binary algorithm test data
        _binaryFullGame6x4Values = BenchmarkTestData.CreateGame6x4PossibleValues();
        _binaryFullGame8x5Values = BenchmarkTestData.CreateGame8x5PossibleValues();
        _binaryReducedGame6x4Values = BenchmarkTestData.CreateReducedPossibleValues(_binaryFullGame6x4Values, 100);
        _binaryReducedGame8x5Values = BenchmarkTestData.CreateReducedPossibleValues(_binaryFullGame8x5Values, 200);
        _binaryTestSelection6x4 = BenchmarkTestData.CreateTestSelection(GameType.Game6x4);
        _binaryTestSelection8x5 = BenchmarkTestData.CreateTestSelection(GameType.Game8x5);
        _colorNames6x4 = BenchmarkTestData.CreateColorNames(GameType.Game6x4);
        _colorNames8x5 = BenchmarkTestData.CreateColorNames(GameType.Game8x5);

        // Initialize string algorithm test data
        _stringFullGame6x4Values = BenchmarkTestData.CreateGame6x4StringPossibleValues();
        _stringFullGame8x5Values = BenchmarkTestData.CreateGame8x5StringPossibleValues();
        _stringReducedGame6x4Values = BenchmarkTestData.CreateReducedStringPossibleValues(_stringFullGame6x4Values, 100);
        _stringReducedGame8x5Values = BenchmarkTestData.CreateReducedStringPossibleValues(_stringFullGame8x5Values, 200);
        _stringTestSelection6x4 = BenchmarkTestData.CreateTestStringSelection(GameType.Game6x4);
        _stringTestSelection8x5 = BenchmarkTestData.CreateTestStringSelection(GameType.Game8x5);
    }

    #region Black Matches Comparison - Game6x4

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game6x4", "Binary")]
    public List<int> Binary_HandleBlackMatches_Game6x4_FullList()
    {
        return _binaryFullGame6x4Values.HandleBlackMatches(GameType.Game6x4, 2, _binaryTestSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game6x4", "String")]
    public List<string[]> String_HandleBlackMatches_Game6x4_FullList()
    {
        return _stringFullGame6x4Values.HandleBlackMatches(GameType.Game6x4, 2, _stringTestSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game6x4", "Binary")]
    public List<int> Binary_HandleBlackMatches_Game6x4_ReducedList()
    {
        return _binaryReducedGame6x4Values.HandleBlackMatches(GameType.Game6x4, 1, _binaryTestSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game6x4", "String")]
    public List<string[]> String_HandleBlackMatches_Game6x4_ReducedList()
    {
        return _stringReducedGame6x4Values.HandleBlackMatches(GameType.Game6x4, 1, _stringTestSelection6x4);
    }

    #endregion

    #region White Matches Comparison - Game6x4

    [Benchmark]
    [BenchmarkCategory("WhiteMatches", "Game6x4", "Binary")]
    public List<int> Binary_HandleWhiteMatches_Game6x4_FullList()
    {
        return _binaryFullGame6x4Values.HandleWhiteMatches(GameType.Game6x4, 2, _binaryTestSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("WhiteMatches", "Game6x4", "String")]
    public List<string[]> String_HandleWhiteMatches_Game6x4_FullList()
    {
        return _stringFullGame6x4Values.HandleWhiteMatches(GameType.Game6x4, 2, _stringTestSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("WhiteMatches", "Game6x4", "Binary")]
    public List<int> Binary_HandleWhiteMatches_Game6x4_ReducedList()
    {
        return _binaryReducedGame6x4Values.HandleWhiteMatches(GameType.Game6x4, 3, _binaryTestSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("WhiteMatches", "Game6x4", "String")]
    public List<string[]> String_HandleWhiteMatches_Game6x4_ReducedList()
    {
        return _stringReducedGame6x4Values.HandleWhiteMatches(GameType.Game6x4, 3, _stringTestSelection6x4);
    }

    #endregion

    #region No Matches Comparison - Game6x4

    [Benchmark]
    [BenchmarkCategory("NoMatches", "Game6x4", "Binary")]
    public List<int> Binary_HandleNoMatches_Game6x4_FullList()
    {
        return _binaryFullGame6x4Values.HandleNoMatches(GameType.Game6x4, _binaryTestSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("NoMatches", "Game6x4", "String")]
    public List<string[]> String_HandleNoMatches_Game6x4_FullList()
    {
        return _stringFullGame6x4Values.HandleNoMatches(GameType.Game6x4, _stringTestSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("NoMatches", "Game6x4", "Binary")]
    public List<int> Binary_HandleNoMatches_Game6x4_ReducedList()
    {
        return _binaryReducedGame6x4Values.HandleNoMatches(GameType.Game6x4, _binaryTestSelection6x4);
    }

    [Benchmark]
    [BenchmarkCategory("NoMatches", "Game6x4", "String")]
    public List<string[]> String_HandleNoMatches_Game6x4_ReducedList()
    {
        return _stringReducedGame6x4Values.HandleNoMatches(GameType.Game6x4, _stringTestSelection6x4);
    }

    #endregion

    #region Game8x5 Comparison

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game8x5", "Binary")]
    public List<int> Binary_HandleBlackMatches_Game8x5_ReducedList()
    {
        return _binaryReducedGame8x5Values.HandleBlackMatches(GameType.Game8x5, 2, _binaryTestSelection8x5);
    }

    [Benchmark]
    [BenchmarkCategory("BlackMatches", "Game8x5", "String")]
    public List<string[]> String_HandleBlackMatches_Game8x5_ReducedList()
    {
        return _stringReducedGame8x5Values.HandleBlackMatches(GameType.Game8x5, 2, _stringTestSelection8x5);
    }

    [Benchmark]
    [BenchmarkCategory("WhiteMatches", "Game8x5", "Binary")]
    public List<int> Binary_HandleWhiteMatches_Game8x5_ReducedList()
    {
        return _binaryReducedGame8x5Values.HandleWhiteMatches(GameType.Game8x5, 3, _binaryTestSelection8x5);
    }

    [Benchmark]
    [BenchmarkCategory("WhiteMatches", "Game8x5", "String")]
    public List<string[]> String_HandleWhiteMatches_Game8x5_ReducedList()
    {
        return _stringReducedGame8x5Values.HandleWhiteMatches(GameType.Game8x5, 3, _stringTestSelection8x5);
    }

    #endregion

    #region Peg Selection Comparison

    [Benchmark]
    [BenchmarkCategory("PegSelection", "Binary")]
    public int Binary_SelectPeg_Game6x4_Position0()
    {
        return _binaryTestSelection6x4.SelectPeg(GameType.Game6x4, 0);
    }

    [Benchmark]
    [BenchmarkCategory("PegSelection", "String")]
    public string String_SelectPeg_Game6x4_Position0()
    {
        return _stringTestSelection6x4.SelectPeg(GameType.Game6x4, 0);
    }

    [Benchmark]
    [BenchmarkCategory("PegSelection", "Binary")]
    public int Binary_SelectPeg_Game8x5_Position4()
    {
        return _binaryTestSelection8x5.SelectPeg(GameType.Game8x5, 4);
    }

    [Benchmark]
    [BenchmarkCategory("PegSelection", "String")]
    public string String_SelectPeg_Game8x5_Position4()
    {
        return _stringTestSelection8x5.SelectPeg(GameType.Game8x5, 4);
    }

    #endregion

    #region Initialization Comparison

    [Benchmark]
    [BenchmarkCategory("Initialization", "Game6x4", "Binary")]
    public List<int> Binary_InitializePossibleValues_Game6x4()
    {
        return BinaryCodeBreakerAlgorithms.GenerateAllPossibleCombinations(GameType.Game6x4, 6);
    }

    [Benchmark]
    [BenchmarkCategory("Initialization", "Game6x4", "String")]
    public List<string[]> String_InitializePossibleValues_Game6x4()
    {
        string[] colors = ["Red", "Blue", "Green", "Yellow", "Orange", "Purple"];
        return StringCodeBreakerAlgorithms.GenerateAllPossibleCombinations(GameType.Game6x4, colors);
    }

    [Benchmark]
    [BenchmarkCategory("Initialization", "Game8x5", "Binary")]
    public List<int> Binary_InitializePossibleValues_Game8x5()
    {
        return BinaryCodeBreakerAlgorithms.GenerateAllPossibleCombinations(GameType.Game8x5, 8);
    }

    [Benchmark]
    [BenchmarkCategory("Initialization", "Game8x5", "String")]
    public List<string[]> String_InitializePossibleValues_Game8x5()
    {
        string[] colors = ["Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Pink", "Brown"];
        return StringCodeBreakerAlgorithms.GenerateAllPossibleCombinations(GameType.Game8x5, colors);
    }

    #endregion

    #region Color Conversion Comparison

    [Benchmark]
    [BenchmarkCategory("ColorConversion", "Binary")]
    public string[] Binary_IntToColors_Game6x4()
    {
        return _binaryTestSelection6x4.IntToColors(GameType.Game6x4, _colorNames6x4);
    }

    [Benchmark]
    [BenchmarkCategory("ColorConversion", "String")]
    public string String_StringToString_Game6x4()
    {
        // For strings, we just return a single color from the selection for comparison
        return _stringTestSelection6x4[0];
    }

    #endregion

    #region Memory Stress Tests

    [Benchmark]
    [BenchmarkCategory("Memory", "Binary", "StressTest")]
    public int Binary_CountLargeList_Game6x4()
    {
        return _binaryFullGame6x4Values.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Memory", "String", "StressTest")]
    public int String_CountLargeList_Game6x4()
    {
        return _stringFullGame6x4Values.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Memory", "Binary", "StressTest")]
    public int Binary_CountLargeList_Game8x5()
    {
        return _binaryFullGame8x5Values.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Memory", "String", "StressTest")]
    public int String_CountLargeList_Game8x5()
    {
        return _stringFullGame8x5Values.Count;
    }

    #endregion
}