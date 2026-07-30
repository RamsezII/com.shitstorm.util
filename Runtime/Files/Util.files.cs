using System.IO;
using UnityEngine;

public static partial class Util
{
    public static DirectoryInfo ForceDir(this string path) => ForceDir(new DirectoryInfo(path));
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