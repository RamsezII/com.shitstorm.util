using System;
using System.IO;
using System.Text.RegularExpressions;
using _UTIL_;

namespace _UTIL_
{
    public enum FS_Types : byte
    {
        File,
        Dir,
    }

    [Flags]
    public enum FS_TYPES : byte
    {
        FILE = 1 << FS_Types.File,
        DIRECTORY = 1 << FS_Types.Dir,
        BOTH = FILE | DIRECTORY,
    }
}

partial class Util
{
    public static bool HasFlags_any(this FS_TYPES mask, FS_TYPES flags) => (mask & flags) != 0;
    public static string ToLinuxPath(this string path) => path.Replace('\\', '/');
    public static bool MatchesPattern(this string value, in string pattern)
    {
        string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(value, regex, RegexOptions.IgnoreCase);
    }

    public static string DOS2UNIX(this string path) => path.Replace("\\", "/");
    public static string DOS2UNIX_full(this string path) => Path.GetFullPath(path).Replace("\\", "/");
}