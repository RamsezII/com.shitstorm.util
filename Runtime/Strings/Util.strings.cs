using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class Util
{
    public static string PullValue(ref string value)
    {
        string temp = value;
        value = temp;
        return temp;
    }

    public static bool TryPullValue(ref string value, out string output)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            output = null;
            return false;
        }
        output = value;
        value = null;
        return true;
    }

    public static void TrimNewline(ref string text)
    {
        if (text.EndsWith('\n'))
            text = text[..^1];
    }

    public static string ReplaceIfNullOrWhiteSpace(this string text, in string replace) => string.IsNullOrWhiteSpace(text) ? replace : text;

    public static string Join(this IEnumerable<string> strings, in string separator, in string prefix = null, in string suffix = null) => prefix + string.Join(separator, strings) + suffix;

    public static string SetAttribute(this string text, in TextB attribute)
    {
        string attr = Util_richtext.rtextAttr[(int)attribute];
        return $"<{attr}>{text}</{attr}>";
    }

    public static string SetAttribute(this string text, in TextB attribute, in string value)
    {
        string attr = Util_richtext.rtextAttr[(int)attribute];
        return $"<{attr}={value}>{text}</{attr}>";
    }

    public static string Mirror(this string text)
    {
        char[] chars = text.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    public static bool TryIndexOf_min(this string text, out int index, in int start_offset, in bool ignore_case, params char[] chars)
    {
        index = text.Length;
        int count = text.Length - start_offset;
        var ordinal = ignore_case.ToOrdinal();

        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            int find = text.IndexOf(c.ToString(), start_offset, count, ordinal);
            if (find >= 0)
                index = Mathf.Min(index, find);
        }

        return index < text.Length;
    }

    public static bool TryIndexOf_max(this string text, out int index, in bool ignore_case = true, params char[] chars)
    {
        index = -1;
        var ordinal = ignore_case.ToOrdinal();

        for (int i = 0; i < chars.Length; i++)
            index = Mathf.Max(index, text.IndexOf(chars[i], ordinal));

        return index >= 0;
    }

    public static StringComparison ToOrdinal(this bool ignore_case) => ignore_case ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}