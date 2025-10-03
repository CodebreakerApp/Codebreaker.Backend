namespace CodeBreaker.Bot.Benchmarks;

/// <summary>
/// Helper class to generate test data for benchmarks
/// </summary>
public static class BenchmarkTestData
{
    /// <summary>
    /// Creates a list of possible values for Game6x4 (similar to InitializePossibleValues6x4)
    /// </summary>
    public static List<int> CreateGame6x4PossibleValues()
    {
        return BinaryCodeBreakerAlgorithms.GenerateAllPossibleCombinations(GameType.Game6x4, 6);
    }

    /// <summary>
    /// Creates a list of possible values for Game8x5
    /// </summary>
    public static List<int> CreateGame8x5PossibleValues()
    {
        return BinaryCodeBreakerAlgorithms.GenerateAllPossibleCombinations(GameType.Game8x5, 8);
    }

    /// <summary>
    /// Creates string-based possible values for Game6x4
    /// </summary>
    public static List<string[]> CreateGame6x4StringPossibleValues()
    {
        string[] colors = ["Red", "Blue", "Green", "Yellow", "Orange", "Purple"];
        return StringCodeBreakerAlgorithms.GenerateAllPossibleCombinations(GameType.Game6x4, colors);
    }

    /// <summary>
    /// Creates string-based possible values for Game8x5
    /// </summary>
    public static List<string[]> CreateGame8x5StringPossibleValues()
    {
        string[] colors = ["Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Pink", "Brown"];
        return StringCodeBreakerAlgorithms.GenerateAllPossibleCombinations(GameType.Game8x5, colors);
    }

    /// <summary>
    /// Creates string-based possible values for Game5x5x4
    /// </summary>
    public static List<string[]> CreateGame5x5x4StringPossibleValues()
    {
        string[] shapeColors = 
        [
            "RedCircle", "RedSquare", "RedTriangle", "RedDiamond", "RedStar",
            "BlueCircle", "BlueSquare", "BlueTriangle", "BlueDiamond", "BlueStar",
            "GreenCircle", "GreenSquare", "GreenTriangle", "GreenDiamond", "GreenStar",
            "YellowCircle", "YellowSquare", "YellowTriangle", "YellowDiamond", "YellowStar",
            "OrangeCircle", "OrangeSquare", "OrangeTriangle", "OrangeDiamond", "OrangeStar"
        ];
        return StringCodeBreakerAlgorithms.GenerateAllPossibleCombinations(GameType.Game5x5x4, shapeColors);
    }

    /// <summary>
    /// Creates a reduced list simulating a game in progress
    /// </summary>
    public static List<int> CreateReducedPossibleValues(List<int> fullList, int targetSize)
    {
        if (fullList.Count <= targetSize)
            return fullList;

        // Create a reduced list by taking every nth element
        var step = fullList.Count / targetSize;
        var reducedList = new List<int>(targetSize);

        for (int i = 0; i < fullList.Count && reducedList.Count < targetSize; i += step)
        {
            reducedList.Add(fullList[i]);
        }

        return reducedList;
    }

    /// <summary>
    /// Creates a reduced string list simulating a game in progress
    /// </summary>
    public static List<string[]> CreateReducedStringPossibleValues(List<string[]> fullList, int targetSize)
    {
        if (fullList.Count <= targetSize)
            return fullList;

        // Create a reduced list by taking every nth element
        var step = fullList.Count / targetSize;
        var reducedList = new List<string[]>(targetSize);

        for (int i = 0; i < fullList.Count && reducedList.Count < targetSize; i += step)
        {
            reducedList.Add(fullList[i]);
        }

        return reducedList;
    }

    /// <summary>
    /// Creates typical selection values for testing
    /// </summary>
    public static int CreateTestSelection(GameType gameType)
    {
        return gameType switch
        {
            GameType.Game6x4 => 0b_000100_000100_000100_000100, // Same color in all positions for 6x4
            GameType.Game8x5 => 0b_000100_000100_000100_000100_000100, // Same color in all positions for 8x5
            GameType.Game5x5x4 => 0b_000100_000100_000100_000100, // Same combination in all positions for 5x5x4
            _ => 0b_000100_000100_000100_000100
        };
    }

    /// <summary>
    /// Creates typical string selection values for testing
    /// </summary>
    public static string[] CreateTestStringSelection(GameType gameType)
    {
        return gameType switch
        {
            GameType.Game6x4 => ["Red", "Red", "Red", "Red"],
            GameType.Game8x5 => ["Red", "Red", "Red", "Red", "Red"],
            GameType.Game5x5x4 => ["RedCircle", "RedCircle", "RedCircle", "RedCircle"],
            _ => ["Red", "Red", "Red", "Red"]
        };
    }

    /// <summary>
    /// Creates color name mappings for algorithms that need them
    /// </summary>
    public static Dictionary<int, string> CreateColorNames(GameType gameType)
    {
        if (gameType == GameType.Game5x5x4)
        {
            // For Game5x5x4, use shape+color combinations
            return new Dictionary<int, string>
            {
                { 1, "RedCircle" }, { 2, "RedSquare" }, { 3, "RedTriangle" }, { 4, "RedDiamond" }, { 5, "RedStar" },
                { 6, "BlueCircle" }, { 7, "BlueSquare" }, { 8, "BlueTriangle" }, { 9, "BlueDiamond" }, { 10, "BlueStar" },
                { 11, "GreenCircle" }, { 12, "GreenSquare" }, { 13, "GreenTriangle" }, { 14, "GreenDiamond" }, { 15, "GreenStar" },
                { 16, "YellowCircle" }, { 17, "YellowSquare" }, { 18, "YellowTriangle" }, { 19, "YellowDiamond" }, { 20, "YellowStar" },
                { 21, "OrangeCircle" }, { 22, "OrangeSquare" }, { 23, "OrangeTriangle" }, { 24, "OrangeDiamond" }, { 25, "OrangeStar" }
            };
        }
        else
        {
            var colors = gameType == GameType.Game8x5
                ? new[] { "Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Pink", "Brown" }
                : new[] { "Red", "Blue", "Green", "Yellow", "Orange", "Purple" };

            var colorMap = new Dictionary<int, string>();
            for (int i = 0; i < colors.Length; i++)
            {
                colorMap[i + 1] = colors[i]; // Map 1-based indices to color names
            }
            return colorMap;
        }
    }
}