using Codebreaker.GameAPIs.Client.Models;

namespace CodeBreaker.BotWithString;

public record struct StringPegWithFlag(string Value, bool Used);

public static class StringCodeBreakerAlgorithms
{
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

    /// <summary>
    /// Reduces the possible values based on the black matches (exact position and color) with the selection
    /// </summary>
    /// <param name="values">The list of possible moves</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="blackHits">The number of black hits with the selection</param>
    /// <param name="selection">The string array of the selected move</param>
    /// <returns>The remaining possible moves</returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<string[]> HandleBlackMatches(this IList<string[]> values, GameType gameType, int blackHits, string[] selection)
    {
        int fieldsCount = GetFieldsCount(gameType);
        int maxMatches = fieldsCount;

        if (blackHits < 0 || blackHits > maxMatches)
        {
            throw new ArgumentException($"invalid argument - hits need to be between 0 and {maxMatches}");
        }

        List<string[]> newValues = new(values.Count);

        foreach (string[] value in values)
        {
            int matches = 0;
            for (int i = 0; i < fieldsCount; i++)
            {
                if (value[i] == selection[i])
                {
                    matches++;
                }
            }

            if (matches == blackHits)
            {
                newValues.Add(value);
            }
        }

        return newValues;
    }

    /// <summary>
    /// Reduces the possible values based on the white matches (correct color, wrong position) with the selection
    /// </summary>
    /// <param name="values">The possible values</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="whiteHits">The number of white hits with the selection</param>
    /// <param name="selection">The selected pegs</param>
    /// <returns>The remaining possible values</returns>
    public static List<string[]> HandleWhiteMatches(this IList<string[]> values, GameType gameType, int whiteHits, string[] selection)
    {
        List<string[]> newValues = new(values.Count);
        int fieldsCount = GetFieldsCount(gameType);

        foreach (string[] value in values)
        {
            // First, create arrays excluding black matches (exact position matches)
            var selectionCopy = new StringPegWithFlag[fieldsCount];
            var valueCopy = new StringPegWithFlag[fieldsCount];
            
            for (int i = 0; i < fieldsCount; i++)
            {
                // If it's a black match (same position, same color), mark as used so it won't count as white
                bool isBlackMatch = value[i] == selection[i];
                selectionCopy[i] = new StringPegWithFlag(selection[i], isBlackMatch);
                valueCopy[i] = new StringPegWithFlag(value[i], isBlackMatch);
            }

            // Now count white matches (same color, different position)
            int whiteMatchCount = 0;
            for (int i = 0; i < fieldsCount; i++)
            {
                if (!valueCopy[i].Used) // Not a black match
                {
                    for (int j = 0; j < fieldsCount; j++)
                    {
                        if (!selectionCopy[j].Used && valueCopy[i].Value == selectionCopy[j].Value)
                        {
                            whiteMatchCount++;
                            selectionCopy[j] = selectionCopy[j] with { Used = true };
                            break; // Only match once
                        }
                    }
                }
            }
            
            if (whiteMatchCount == whiteHits)
            {
                newValues.Add(value);
            }
        }

        return newValues;
    }

    /// <summary>
    /// Reduces the possible values based on the blue matches (partial matches for Game5x5x4) with the selection
    /// </summary>
    /// <param name="values">The possible values</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="blueHits">The number of blue hits with the selection</param>
    /// <param name="selection">The selected pegs</param>
    /// <returns>The remaining possible values</returns>
    public static List<string[]> HandleBlueMatches(this IList<string[]> values, GameType gameType, int blueHits, string[] selection)
    {
        // Blue matches only apply to Game5x5x4
        if (gameType != GameType.Game5x5x4)
        {
            return values.ToList(); // No filtering needed for other game types
        }

        List<string[]> newValues = new(values.Count);
        int fieldsCount = GetFieldsCount(gameType);
        
        foreach (string[] value in values)
        {
            // For Game5x5x4, we need to count partial matches
            // This is a simplified implementation that counts blue-like matches
            // In a real implementation, this would need to understand shape+color combinations
            // For now, we'll do a basic filtering that reduces possibilities
            
            int partialMatches = 0;
            for (int i = 0; i < fieldsCount; i++)
            {
                string selectionField = selection[i];
                string valueField = value[i];
                
                // This is a simplified blue match check
                // In reality, blue matches are more complex for shape+color combinations
                // For string-based implementation, we'll check if they share some common characteristics
                // but are not exactly the same
                if (valueField != selectionField && HasPartialMatch(valueField, selectionField))
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
    /// Helper method to determine if two strings have a partial match (for blue hits in Game5x5x4)
    /// </summary>
    /// <param name="value">The value string</param>
    /// <param name="selection">The selection string</param>
    /// <returns>True if there's a partial match</returns>
    private static bool HasPartialMatch(string value, string selection)
    {
        // This is a simplified implementation for partial matching
        // In a real Game5x5x4 implementation, this would check shape+color combinations
        // For now, we'll implement a simple string-based partial match
        
        // If strings are the same, it's not a partial match (that would be a black match)
        if (value == selection)
            return false;
            
        // Check if they have any common characters (simplified partial match logic)
        return value.Any(c => selection.Contains(c));
    }

    /// <summary>
    /// Reduces the possible values by removing those that contain any of the colors from the selection
    /// </summary>
    /// <param name="values">The possible values</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="selection">The selected pegs</param>
    /// <returns>The remaining possible values</returns>
    public static List<string[]> HandleNoMatches(this IList<string[]> values, GameType gameType, string[] selection)
    {
        bool ContainsAnySelectionColor(string[] value, string[] selections)
        {
            foreach (string valueColor in value)
            {
                if (selections.Contains(valueColor))
                {
                    return true;
                }
            }
            return false;
        }

        List<string[]> newValues = new(values.Count);

        foreach (string[] value in values)
        {
            if (!ContainsAnySelectionColor(value, selection))
            {
                newValues.Add(value);
            }
        }
        return newValues;
    }

    /// <summary>
    /// Get a specific peg from the string array representation
    /// </summary>
    /// <param name="codes">The string array representing the pegs</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="pegNumber">The peg number to retrieve</param>
    /// <returns>The string value of the selected peg</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string SelectPeg(this string[] codes, GameType gameType, int pegNumber)
    {
        int fieldsCount = GetFieldsCount(gameType);

        if (pegNumber < 0 || pegNumber >= fieldsCount)
            throw new InvalidOperationException($"invalid peg number {pegNumber}");

        if (codes.Length != fieldsCount)
            throw new InvalidOperationException($"codes array length {codes.Length} does not match expected fields count {fieldsCount}");

        return codes[pegNumber];
    }

    /// <summary>
    /// Generate all possible combinations for the given game type and field values
    /// </summary>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="fieldValues">The possible values for each field</param>
    /// <returns>A list of all possible combinations</returns>
    public static List<string[]> GenerateAllPossibleCombinations(GameType gameType, string[] possibleValues)
    {
        int fieldsCount = GetFieldsCount(gameType);
        List<string[]> combinations = new();

        GenerateCombinationsRecursive(combinations, new string[fieldsCount], 0, fieldsCount, possibleValues);

        return combinations;
    }

    /// <summary>
    /// Recursive helper method to generate all possible combinations
    /// </summary>
    private static void GenerateCombinationsRecursive(List<string[]> combinations, string[] current, int position, int fieldsCount, string[] possibleValues)
    {
        if (position == fieldsCount)
        {
            combinations.Add((string[])current.Clone());
            return;
        }

        foreach (string value in possibleValues)
        {
            current[position] = value;
            GenerateCombinationsRecursive(combinations, current, position + 1, fieldsCount, possibleValues);
        }
    }
}