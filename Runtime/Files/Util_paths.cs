using _UTIL_;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
    public readonly static StringComparison comp_path = is_windows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    //----------------------------------------------------------------------------------------------------------

    public static bool HasFlags_any(this FS_TYPES mask, FS_TYPES flags) => (mask & flags) != 0;
    public static string ToLinuxPath(this string path) => path.Replace('\\', '/');
    public static bool MatchesPattern(this string value, in string pattern)
    {
        string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(value, regex, RegexOptions.IgnoreCase);
    }

    public static bool IsSamePath_full(this string a, in string b) => string.Equals(NormalizePath(a), NormalizePath(b), comp_path);

    public static bool IsSameDir(this DirectoryInfo a, DirectoryInfo b) => a.FullName.Equals_path(b.FullName);

    public static string NormalizePath(this string full_path)
    {
        full_path = Path.GetFullPath(full_path).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        string root = Path.GetPathRoot(full_path);

        full_path = (full_path.EndsWith(Path.DirectorySeparatorChar) && full_path != root)
            ? full_path.TrimEnd(Path.DirectorySeparatorChar)
            : full_path;

        return full_path;
    }

    public static string CombinePaths(params string[] paths) => Path.Combine(paths).NormalizePath();

    public static bool IsParentDirectoryOf(this string parent, string candidate)
    {
        parent = NormalizePath(parent);
        candidate = NormalizePath(candidate);

        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(candidate))
            return false;

        // Racines différentes => pas descendant (ex: "D:\..." vs "C:\...") sur Windows
        string rootA = Path.GetPathRoot(parent);
        string rootB = Path.GetPathRoot(candidate);

        if (!string.Equals(rootA, rootB, comp_path))
            return false;

        // Forcer un séparateur terminal sur le parent pour éviter les faux positifs
        // (ex: "C:\Foo" ne doit pas matcher "C:\Foobar")
        string parentWithSep = parent.EndsWith(Path.DirectorySeparatorChar.ToString())
            ? parent
            : parent + Path.DirectorySeparatorChar;

        return candidate.StartsWith(parentWithSep, comp_path) || IsSamePath_full(parent, candidate);
    }
}