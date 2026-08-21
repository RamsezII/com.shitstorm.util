using System;
using System.Collections.Generic;

public static partial class Util
{
    public static IEnumerable<T> EMatchChars<T>(this string chars, IEnumerable<T> values)
    {
        foreach (var s in values)
            if (IsMatchChars(chars, s.ToString()))
                yield return s;
    }

    public static bool IsMatchChars(this string chars, string text)
    {
        Queue<char> set = new(chars);
        while (set.TryDequeue(out char c))
        {
            int index = text.IndexOf(c, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return false;
            text = text[index..];
        }
        return true;
    }
}