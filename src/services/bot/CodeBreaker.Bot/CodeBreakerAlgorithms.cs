using System.Runtime.CompilerServices;
using Codebreaker.GameAPIs.Client.Models;

namespace CodeBreaker.Bot;

public record struct KeyPegWithFlag(int Value, bool Used);

public static class CodeBreakerAlgorithms
{
    // definitions to mask the different pegs
    private const int C0001 = 0b_111111;
    private const int C0010 = 0b_111111_000000;
    private const int C0100 = 0b_111111_000000_000000;
    private const int C1000 = 0b_111111_000000_000000_000000;

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
        int maxHits = GetFieldsCount(gameType) - 1;
        if (blackHits is < 1 || blackHits > maxHits)
        {
            throw new ArgumentException($"invalid argument - hits need to be between 1 and {maxHits}");
        }

        bool IsMatch(int value, int blackhits, int selection)
        {
            int fieldsCount = GetFieldsCount(gameType);
            int bitsPerField = GetBitsPerField(gameType);
            int mask = (1 << bitsPerField) - 1;
            
            int matches = 0;
            for (int field = 0; field < fieldsCount; field++)
            {
                int shift = field * bitsPerField;
                int selectionField = (selection >> shift) & mask;
                int valueField = (value >> shift) & mask;
                
                if (valueField == selectionField)
                {
                    matches++;
                }
            }
            return (matches == blackhits);
        }

        List<int> result = new(capacity: values.Count);

        foreach (int value in values)
        {
            if (IsMatch(value, blackHits, selection))
            {
                result.Add(value);
            }
        }
        return result;
    }

    /// <summary>
    /// Reduces the possible values based on the white matches with the selection
    /// </summary>
    /// <param name="values">The possible values</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="whiteHits">The number of white hits with the selection</param>
    /// <param name="selection">The selected pegs</param>
    /// <returns>The remaining possbile values</returns>
    public static List<int> HandleWhiteMatches(this IList<int> values, GameType gameType, int whiteHits, int selection)
    {
        List<int> newValues = new(values.Count);
        int fieldsCount = GetFieldsCount(gameType);
        
        foreach (int value in values)
        {
            // need to have clean selections with every run
            var selections = new KeyPegWithFlag[fieldsCount];
            for (int i = 0; i < fieldsCount; i++)
            {
                selections[i] = new KeyPegWithFlag(selection.SelectPeg(gameType, i), false);
            }

            var matches = new KeyPegWithFlag[fieldsCount];
            for (int i = 0; i < fieldsCount; i++)
            {
                matches[i] = new KeyPegWithFlag(value.SelectPeg(gameType, i), false);
            }

            int matchCount = 0;
            for (int i = 0; i < fieldsCount; i++)
            {
                for (int j = 0; j < fieldsCount; j++)
                {
                    if (!matches[i].Used && !selections[j].Used && matches[i].Value == selections[j].Value)
                    {
                        matchCount++;
                        selections[j] = selections[j] with { Used = true };
                        matches[i] = matches[i] with { Used = true };
                    }
                }
            }
            if (matchCount == whiteHits)
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
    /// <returns>The remaining possbile values</returns>
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
            // For Game5x5x4, we need to count partial matches
            // This is a simplified implementation that counts blue-like matches
            // In a real implementation, this would need to understand shape+color combinations
            // For now, we'll do a basic filtering that reduces possibilities
            
            int partialMatches = 0;
            for (int i = 0; i < fieldsCount; i++)
            {
                int selectionField = selection.SelectPeg(gameType, i);
                int valueField = value.SelectPeg(gameType, i);
                
                // This is a simplified blue match check
                // In reality, blue matches are more complex for shape+color combinations
                if (valueField != selectionField && (valueField & selectionField) != 0)
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
    /// </summary>
    /// <param name="values">The possible values</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="selection">The selected pegs</param>
    /// <returns>The remaining possbile values</returns>
    public static List<int> HandleNoMatches(this IList<int> values, GameType gameType, int selection)
    {
        bool Contains(int[] selections, int value)
        {
            int fieldsCount = GetFieldsCount(gameType);
            for (int i = 0; i < fieldsCount; i++)
            {
                if (selections.Contains(value.SelectPeg(gameType, i)))
                {
                    return true;
                }
            }
            return false;
        }

        List<int> newValues = new(values.Count);
        int fieldsCount = GetFieldsCount(gameType);
        int[] selections = Enumerable.Range(0, fieldsCount)
            .Select(i => selection.SelectPeg(gameType, i))
            .ToArray();

        foreach (int value in values)
        {
            if (!Contains(selections, value))
            {
                newValues.Add(value);
            }
        }
        return newValues;
    }

    /// <summary>
    /// Get the int representation of one peg from the int representaiton of pegs
    /// </summary>
    /// <param name="code">The int value representing the pegs</param>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="pegNumber">The peg number to retrieve from the int representation</param>
    /// <returns>The int value of the selected peg</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static int SelectPeg(this int code, GameType gameType, int pegNumber)
    {
        int bitsPerField = GetBitsPerField(gameType);
        int fieldsCount = GetFieldsCount(gameType);
        int mask = (1 << bitsPerField) - 1;

        if (pegNumber < 0 || pegNumber >= fieldsCount)
            throw new InvalidOperationException($"invalid peg number {pegNumber}");

        int shift = pegNumber * bitsPerField;
        return (code >> shift) & mask;
    }

    private static int GetFieldsCount(GameType gameType) =>
        gameType switch
        {
            GameType.Game6x4 => 4,
            GameType.Game8x5 => 5,
            GameType.Game5x5x4 => 4,
            _ => 4
        };

    private static int GetBitsPerField(GameType gameType) =>
        gameType switch
        {
            GameType.Game6x4 => 6,  // 6 bits for up to 64 values (using 6)
            GameType.Game8x5 => 6,  // 6 bits for up to 64 values (using 8)  
            GameType.Game5x5x4 => 6, // 6 bits for up to 64 values (using 25, but limited per position)
            _ => 6
        };
}