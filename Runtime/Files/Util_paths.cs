using System;
using System.IO;

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

    public static DirectoryInfo Combine(this DirectoryInfo directory, params string[] dir_names)
    {
        string[] temp = new string[1 + dir_names.Length];
        temp[0] = directory.FullName;
        for (int i = 0; i < dir_names.Length; i++)
            temp[1 + i] = dir_names[i];
        string combine = Path.Combine(temp);
        return new(combine);
    }

    public static bool IsSamePath_full(this string a, in string b) => string.Equals(NormalizePath(a), NormalizePath(b), comp_path);

    public static string NormalizePath(this string full_path)
    {
        full_path = Path.GetFullPath(full_path).ForceLinuxPathSeparators();

        string root = Path.GetPathRoot(full_path);

        full_path = (full_path.EndsWith(Path.DirectorySeparatorChar) && full_path != root)
            ? full_path.TrimEnd(Path.DirectorySeparatorChar)
            : full_path;

        return full_path;
    }

    public static string ForceLinuxPathSeparators(this string path) => path.Replace('\\', '/');

    public static string Dos2Unix(this string text) => text.Replace("\r\n", "\n").Replace('\r', '\n');
}