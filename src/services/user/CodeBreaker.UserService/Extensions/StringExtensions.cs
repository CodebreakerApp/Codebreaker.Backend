using System.IO;

namespace CodeBreaker.UserService.Extensions;

internal static class StringExtensions
{
    public static string ToUpperFirstChar(this string s)
    {
        var chars = s.ToCharArray();
        chars[0] = char.ToUpper(chars[0]);
        return new string(chars);
    }
}
