namespace CodeBreaker.Bot;

public record struct KeyPegWithFlag(int Value, bool Used);

public static class CodeBreakerAlgorithms
{
    private const int c0001 = 0b_111111;
    private const int c0010 = 0b_111111_000000;
    private const int c0100 = 0b_111111_000000_000000;
    private const int c1000 = 0b_111111_000000_000000_000000;

    public static string[] IntToColors(this int value)
    {
        int i1 = (value >> 0) & 0b111111;
        int i2 = (value >> 6) & 0b111111;
        int i3 = (value >> 12) & 0b111111;
        int i4 = (value >> 18) & 0b111111;
        CodeColors c1 = (CodeColors)i1;
        CodeColors c2 = (CodeColors)i2;
        CodeColors c3 = (CodeColors)i3;
        CodeColors c4 = (CodeColors)i4;
        var colorNames = new string[]
        {
            c4.ToString(), c3.ToString(), c2.ToString(), c1.ToString()
        };

        return colorNames;
    }

    public static List<int> HandleBlackMatches(this IList<int> values, int blackHits, int selection)
    {
        if (blackHits < 1 || blackHits > 3)
        {
            throw new ArgumentException("invalid argument - hits need to be between 1 and 3");
        }

        static bool IsMatch(int value, int blackhits, int selection)
        {
            int n1 = selection & c0001;
            int n2 = selection & c0010;
            int n3 = selection & c0100;
            int n4 = selection & c1000;
            int matches = 0;
            bool match1 = (value & n1) == n1;
            if (match1) matches++;
            if ((value & n2) == n2) matches++;
            if ((value & n3) == n3) matches++;
            if ((value & n4) == n4) matches++;
            return (matches == blackhits);
        }

        List<int> result = new(capacity: values.Count);

        foreach (var value in values)
        {
            if (IsMatch(value, blackHits, selection))
            {
                result.Add(value);
            }
        }
        return result;
    }

    public static List<int> HandleWhiteMatches(this IList<int> values, int allHits, int selection)
    {
        List<int> newValues = new(values.Count);
        foreach (var value in values)
        {
            // need to have clean selections with every run
            KeyPegWithFlag[] selections = new KeyPegWithFlag[4];
            for (int i = 0; i < 4; i++)
            {
                selections[i] = new KeyPegWithFlag(selection.SelectPeg(i), false);
            }

            KeyPegWithFlag[] matches = new KeyPegWithFlag[4];
            for (int i = 0; i < 4; i++)
            {
                matches[i] = new KeyPegWithFlag(value.SelectPeg(i), false);
            }

            int matchCount = 0;
            for (int i = 0; i < 4; i++)
            {

                for (int j = 0; j < 4; j++)
                {
                    if (!matches[i].Used && !selections[j].Used && matches[i].Value == selections[j].Value)
                    {
                        matchCount++;
                        selections[j] = selections[j] with { Used = true };
                        matches[i] = matches[i] with { Used = true };
                    }
                }

            }
            if (matchCount == allHits)
            {
                newValues.Add(value);
            }
        }

        return newValues;
    }

    public static short ColorToBits(this string color)
    {
        return color switch
        {
            "black" => 1,
            "white" => 2,
            "red" => 4,
            "green" => 8,
            "blue" => 16,
            "yellow" => 32,
            _ => throw new InvalidOperationException("invalid color string")
        };
    }

    public static string BitsToColorString(this int s) => s switch
    {
        1 => "black",
        2 => "white",
        4 => "red",
        8 => "green",
        16 => "blue",
        32 => "yellow",
        _ => throw new InvalidOperationException("invalid color number")
    };

    public static int SelectPeg(this int code, int pegNumber) => pegNumber switch
    {
        0 => code & 0b111111,
        1 => (code >> 6) & 0b111111,
        2 => (code >> 12) & 0b111111,
        3 => (code >> 18) & 0b111111,
        _ => throw new InvalidOperationException("invalid peg number")
    };
}
