using System;
using System.IO;

public static partial class Util
{
    public static bool TryGetSplit(this string text, in Index index, out string split) => TryGetSplit(text, index, out split, out _);
    public static bool TryGetSplit(this string text, in Index index, out string split, out string[] splits)
    {
        splits = text.Split('.');
        if (index.Value < splits.Length)
        {
            split = splits[index];
            return true;
        }
        else
        {
            split = string.Empty;
            return false;
        }
    }

    public static string GetFileName(this string path)
    {
        if (TryGetSplit(Path.GetFileName(path), 0, out string split))
            return split;
        else
            return string.Empty;
    }

    public static void CheckParentDirectory(this string path) => CheckDirectory(Directory.GetParent(path).FullName);
    public static void CheckDirectory(this string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public static bool EndsWith(this string text, params char[] chars)
    {
        foreach (char c in chars)
            if (text[^1] == c)
                return true;
        return false;
    }

    public static bool Contains2(this string text, in string chars)
    {
        foreach (char c in chars)
            if (text.Contains(c))
                return true;
        return false;
    }
}