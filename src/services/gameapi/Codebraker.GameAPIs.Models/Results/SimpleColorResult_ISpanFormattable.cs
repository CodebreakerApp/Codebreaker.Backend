namespace CodeBreaker.GameAPIs.Models;
public readonly partial struct SimpleColorResult :  ISpanFormattable
{
    public string ToString(string? format = default, IFormatProvider? formatProvider = default)
    {
        char[] buffer = new char[7];
        if (TryFormat(buffer.AsSpan(), out int _))
        {
            return new string(buffer);
        }
        else
        {
            throw new FormatException();
        }
    }

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format = default,
        IFormatProvider? provider = default)
    {
        if (destination.Length < 7)
        {
            charsWritten = 0;
            return false;
        }

        for (int i = 0, j = 0; i < 4; i++, j += 2)
        {
            destination[j] = (char)((byte)_results[i] + '0'); 
            if (j < 6) destination[j + 1] = ':';
        }
        charsWritten = 7;
        return true;
    }
}
