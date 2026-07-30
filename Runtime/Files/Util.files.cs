using System;
using System.IO;
using UnityEngine;

public static partial class Util
{
    public static bool Equals_path(this string path1, string path2) => Path.GetFullPath(path1).Equals(Path.GetFullPath(path2), StringComparison.OrdinalIgnoreCase);

    public static FileInfo GetFile(this string path) => new(path);
    public static DirectoryInfo ForceDir(this DirectoryInfo dir)
    {
        if (dir.Exists)
            return dir;
        Directory.CreateDirectory(dir.FullName);
        return new(dir.FullName);
    }

    public static DirectoryInfo GetDir(this string path, in bool force)
    {
        DirectoryInfo dir = new(path);
        if (force && !dir.Exists)
        {
            dir.Create();
            Debug.Log($"pushed creation dir: \"{dir.FullName}\"".ToSubLog());
        }
        return dir;
    }
}