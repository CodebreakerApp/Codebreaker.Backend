using System.Runtime.CompilerServices;

namespace CodeBreaker.Bot.Benchmarks;

public record struct KeyPegWithFlag(int Value, bool Used);

/// <summary>
/// Local copy of binary CodeBreaker algorithms for benchmarking
/// </summary>
public static class BinaryCodeBreakerAlgorithms
{
    // definitions to mask the different pegs
    private const int C0001 = 0b_111111;
    private const int C0010 = 0b_111111_000000;
    private const int C0100 = 0b_111111_000000_000000;
    private const int C1000 = 0b_111111_000000_000000_000000;

    /// <summary>
    /// Get the number of fields/codes for the specified game type
    /// </summary>
    /// <param name="gameType">The type of game being played</param>
    /// <returns>The number of fields/codes</returns>
    private static int GetFieldsCount(GameType gameType) =>
        gameType switch
        {
            GameType.Game6x4 => 4,
            GameType.Game8x5 => 5,
            GameType.Game5x5x4 => 4,
            _ => 4
        };

    // Convert the int representation of pegs to an array of color names
    public static string[] IntToColors(this int value, GameType gameType, Dictionary<int, string>? colorNames)
    {
        return gameType switch
        {
            GameType.Game6x4 => IntToColors6x4(value, colorNames),
            GameType.Game8x5 => IntToColors8x5(value, colorNames),
            GameType.Game5x5x4 => IntToColors5x5x4(value, colorNames),
            _ => IntToColors6x4(value, colorNames)
        };
    }

    private static string[] IntToColors6x4(int value, Dictionary<int, string>? colorNames)
    {
        int i1 = (value >> 0) & 0b111111;
        int i2 = (value >> 6) & 0b111111;
        int i3 = (value >> 12) & 0b111111;
        int i4 = (value >> 18) & 0b111111;
        
        string[] colorNamesArray =
        [
            colorNames?[i4] ?? $"Unknown{i4}",
            colorNames?[i3] ?? $"Unknown{i3}",
            colorNames?[i2] ?? $"Unknown{i2}",
            colorNames?[i1] ?? $"Unknown{i1}"
        ];

        return colorNamesArray;
    }

    private static string[] IntToColors8x5(int value, Dictionary<int, string>? colorNames)
    {
        int i1 = (value >> 0) & 0b111111;
        int i2 = (value >> 6) & 0b111111;
        int i3 = (value >> 12) & 0b111111;
        int i4 = (value >> 18) & 0b111111;
        int i5 = (value >> 24) & 0b111111;
        
        string[] colorNamesArray =
        [
            colorNames?[i5] ?? $"Unknown{i5}",
            colorNames?[i4] ?? $"Unknown{i4}",
            colorNames?[i3] ?? $"Unknown{i3}",
            colorNames?[i2] ?? $"Unknown{i2}",
            colorNames?[i1] ?? $"Unknown{i1}"
        ];

        return colorNamesArray;
    }

    private static string[] IntToColors5x5x4(int value, Dictionary<int, string>? colorNames)
    {
        int i1 = (value >> 0) & 0b111111;
        int i2 = (value >> 6) & 0b111111;
        int i3 = (value >> 12) & 0b111111;
        int i4 = (value >> 18) & 0b111111;
        
        string[] colorNamesArray =
        [
            colorNames?[i4] ?? $"Unknown{i4}",
            colorNames?[i3] ?? $"Unknown{i3}",
            colorNames?[i2] ?? $"Unknown{i2}",
            colorNames?[i1] ?? $"Unknown{i1}"
        ];

        return colorNamesArray;
    }

    /// <summary>
    /// Reduces the possible values based on the black matches with the selection
    /// </summary>
    /// <param name="values">The list of possible moves</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="blackHits">The number of black hits with the selection</param>
    /// <param name="selection">The key pegs of the selected move</param>
    /// <returns>The remaining possible moves</returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<int> HandleBlackMatches(this IList<int> values, GameType gameType, int blackHits, int selection)
    {
        int maxHits = GetFieldsCount(gameType);
        if (blackHits < 0 || blackHits >= maxHits)
        {
            throw new ArgumentException($"invalid argument - hits need to be between 0 and {maxHits - 1}");
        }

        List<int> newValues = new(values.Count);

        foreach (int value in values)
        {
            int exactMatches = CountExactMatches(value, selection, gameType);
            if (exactMatches == blackHits)
            {
                newValues.Add(value);
            }
        }

        return newValues;
    }

    /// <summary>
    /// Reduces the possible values based on the white matches with the selection
    /// </summary>
    /// <param name="values">The possible values</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="whiteHits">The number of white hits with the selection</param>
    /// <param name="selection">The selected pegs</param>
    /// <returns>The remaining possible values</returns>
    public static List<int> HandleWhiteMatches(this IList<int> values, GameType gameType, int whiteHits, int selection)
    {
        List<int> newValues = new(values.Count);
        int fieldsCount = GetFieldsCount(gameType);

        foreach (int value in values)
        {
            int exactMatches = CountExactMatches(value, selection, gameType);
            int totalMatches = CountTotalMatches(value, selection, gameType);
            int calculatedWhiteMatches = totalMatches - exactMatches;

            if (calculatedWhiteMatches == whiteHits)
            {
                newValues.Add(value);
            }
        }

        return newValues;
    }

    /// <summary>
    /// Reduces the possible values based on the blue matches with the selection
    /// </summary>
    /// <param name="values">The possible values</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="blueHits">The number of blue hits with the selection</param>
    /// <param name="selection">The selected pegs</param>
    /// <returns>The remaining possible values</returns>
    public static List<int> HandleBlueMatches(this IList<int> values, GameType gameType, int blueHits, int selection)
    {
        // Blue matches only apply to Game5x5x4
        if (gameType != GameType.Game5x5x4)
        {
            return values.ToList(); // No filtering needed for other game types
        }

        List<int> newValues = new(values.Count);
        int fieldsCount = GetFieldsCount(gameType);

        foreach (int value in values)
        {
            // For Game5x5x4, this is a simplified implementation
            // In reality, blue matches are more complex for shape+color combinations
            int partialMatches = 0;
            
            for (int i = 0; i < fieldsCount; i++)
            {
                int valuePeg = value.SelectPeg(gameType, i);
                int selectionPeg = selection.SelectPeg(gameType, i);
                
                // Simplified partial match logic
                if (valuePeg != selectionPeg && HasPartialMatch(valuePeg, selectionPeg))
                {
                    partialMatches++;
                }
            }

            if (partialMatches == blueHits)
            {
                newValues.Add(value);
            }
        }

        return newValues;
    }

    /// <summary>
    /// Helper method for simplified partial matching
    /// </summary>
    private static bool HasPartialMatch(int value, int selection)
    {
        // Simplified implementation - in reality would be more complex
        return (value & 0b111) == (selection & 0b111) || (value >> 3) == (selection >> 3);
    }

    /// <summary>
    /// Reduces the possible values by removing those that contain any of the colors from the selection
    /// </summary>
    /// <param name="values">The possible values</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="selection">The selected pegs</param>
    /// <returns>The remaining possible values</returns>
    public static List<int> HandleNoMatches(this IList<int> values, GameType gameType, int selection)
    {
        List<int> newValues = new(values.Count);

        foreach (int value in values)
        {
            if (!ContainsAnySelectionColor(value, selection, gameType))
            {
                newValues.Add(value);
            }
        }
        return newValues;
    }

    /// <summary>
    /// Get a specific peg from the binary representation
    /// </summary>
    /// <param name="code">The binary representation of the pegs</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="pegNumber">The peg number to retrieve</param>
    /// <returns>The binary value of the selected peg</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static int SelectPeg(this int code, GameType gameType, int pegNumber)
    {
        int fieldsCount = GetFieldsCount(gameType);

        if (pegNumber < 0 || pegNumber >= fieldsCount)
            throw new InvalidOperationException($"invalid peg number {pegNumber}");

        return (code >> (pegNumber * 6)) & 0b111111;
    }

    /// <summary>
    /// Generate all possible combinations for the given game type and number of colors
    /// </summary>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="numberOfColors">The number of colors available</param>
    /// <returns>A list of all possible combinations as int values</returns>
    public static List<int> GenerateAllPossibleCombinations(GameType gameType, int numberOfColors)
    {
        int fieldsCount = GetFieldsCount(gameType);
        List<int> combinations = new();

        int totalCombinations = (int)Math.Pow(numberOfColors, fieldsCount);
        
        for (int i = 0; i < totalCombinations; i++)
        {
            int combination = 0;
            int temp = i;
            
            for (int pos = 0; pos < fieldsCount; pos++)
            {
                int colorIndex = temp % numberOfColors;
                combination |= ((colorIndex + 1) << (pos * 6)); // +1 to avoid 0 values
                temp /= numberOfColors;
            }
            
            combinations.Add(combination);
        }

        return combinations;
    }

    #region Helper Methods

    private static int CountExactMatches(int value, int selection, GameType gameType)
    {
        int fieldsCount = GetFieldsCount(gameType);
        int exactMatches = 0;

        for (int i = 0; i < fieldsCount; i++)
        {
            if (value.SelectPeg(gameType, i) == selection.SelectPeg(gameType, i))
            {
                exactMatches++;
            }
        }

        return exactMatches;
    }

    private static int CountTotalMatches(int value, int selection, GameType gameType)
    {
        int fieldsCount = GetFieldsCount(gameType);
        
        // Extract all pegs from both values
        var valueArray = new KeyPegWithFlag[fieldsCount];
        var selectionArray = new KeyPegWithFlag[fieldsCount];

        for (int i = 0; i < fieldsCount; i++)
        {
            valueArray[i] = new KeyPegWithFlag(value.SelectPeg(gameType, i), false);
            selectionArray[i] = new KeyPegWithFlag(selection.SelectPeg(gameType, i), false);
        }

        int totalMatches = 0;
        
        // Count matches
        for (int i = 0; i < fieldsCount; i++)
        {
            if (!valueArray[i].Used)
            {
                for (int j = 0; j < fieldsCount; j++)
                {
                    if (!selectionArray[j].Used && valueArray[i].Value == selectionArray[j].Value)
                    {
                        totalMatches++;
                        valueArray[i] = valueArray[i] with { Used = true };
                        selectionArray[j] = selectionArray[j] with { Used = true };
                        break;
                    }
                }
            }
        }

        return totalMatches;
    }

    private static bool ContainsAnySelectionColor(int value, int selection, GameType gameType)
    {
        int fieldsCount = GetFieldsCount(gameType);
        
        var selectionPegs = new HashSet<int>();
        for (int i = 0; i < fieldsCount; i++)
        {
            selectionPegs.Add(selection.SelectPeg(gameType, i));
        }

        for (int i = 0; i < fieldsCount; i++)
        {
            if (selectionPegs.Contains(value.SelectPeg(gameType, i)))
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}