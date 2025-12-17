using System.Collections.Generic;
using System.Text;

partial class Util
{
    public static string RemoveChar(this string input, char c)
    {
        StringBuilder sb = new();
        for (int i = 0; i < input.Length; i++)
            if (input[i] != c)
                sb.Append(input[i]);
        return sb.ToString();
    }

    public static string RemoveChars(this string input, string chars)
    {
        HashSet<char> _chars = new(chars);
        StringBuilder sb = new();
        for (int i = 0; i < input.Length; i++)
            if (!_chars.Contains(input[i]))
                sb.Append(input[i]);
        return sb.ToString();
    }
}