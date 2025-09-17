using Codebreaker.GameAPIs.Client.Models;

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
        static List<int> CreateColors(int colorCount, int shift)
        {
            List<int> pin = [];
            for (int i = 0; i < colorCount; i++)
            {
                int x = 1 << i + shift;
                pin.Add(x);
            }
            return pin;
        }

        static List<int> AddColorsToList(List<int> list1, List<int> list2)
        {
            List<int> result = new(capacity: 1300);
            for (int i = 0; i < list1.Count; i++)
            {
                for (int j = 0; j < list2.Count; j++)
                {
                    int x = list1[i] ^ list2[j];
                    result.Add(x);
                }
            }
            return result;
        }

        var digits1 = CreateColors(6, 0);
        var digits2 = CreateColors(6, 6);
        var list2 = AddColorsToList(digits1, digits2);
        var digits3 = CreateColors(6, 12);
        var list3 = AddColorsToList(list2, digits3);
        var digits4 = CreateColors(6, 18);
        var list4 = AddColorsToList(list3, digits4);
        list4.Sort();
        return list4;
    }

    /// <summary>
    /// Creates a list of possible values for Game8x5
    /// </summary>
    public static List<int> CreateGame8x5PossibleValues()
    {
        static List<int> Create8Colors(int shift)
        {
            List<int> pin = [];
            for (int i = 0; i < 8; i++)
            {
                int x = 1 << (i + shift);
                pin.Add(x);
            }
            return pin;
        }

        static List<int> AddColorsToList(List<int> list1, List<int> list2)
        {
            List<int> result = new(capacity: list1.Count * list2.Count);
            for (int i = 0; i < list1.Count; i++)
            {
                for (int j = 0; j < list2.Count; j++)
                {
                    int x = list1[i] ^ list2[j];
                    result.Add(x);
                }
            }
            return result;
        }

        var digits1 = Create8Colors(0);
        var digits2 = Create8Colors(6);
        var list2 = AddColorsToList(digits1, digits2);
        var digits3 = Create8Colors(12);
        var list3 = AddColorsToList(list2, digits3);
        var digits4 = Create8Colors(18);
        var list4 = AddColorsToList(list3, digits4);
        var digits5 = Create8Colors(24);
        var list5 = AddColorsToList(list4, digits5);
        list5.Sort();
        return list5;
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
    /// Creates color name mappings for testing
    /// </summary>
    public static Dictionary<int, string> CreateColorNames(GameType gameType)
    {
        var colorNames = new Dictionary<int, string>();
        int key = 1;

        if (gameType == GameType.Game5x5x4)
        {
            // Create shape+color combinations
            var colors = new[] { "Red", "Green", "Blue", "Yellow", "Orange" };
            var shapes = new[] { "Circle", "Square", "Triangle", "Diamond", "Star" };

            foreach (var shape in shapes)
            {
                foreach (var color in colors)
                {
                    colorNames[key] = $"{shape};{color}";
                    key <<= 1;
                }
            }
        }
        else
        {
            // Color-only games
            var colors = gameType == GameType.Game8x5
                ? new[] { "Red", "Green", "Blue", "Yellow", "Orange", "Purple", "Pink", "White" }
                : new[] { "Red", "Green", "Blue", "Yellow", "Orange", "Purple" };

            foreach (var color in colors)
            {
                colorNames[key] = color;
                key <<= 1;
            }
        }

        return colorNames;
    }
}