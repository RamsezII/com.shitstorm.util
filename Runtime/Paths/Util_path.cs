using System.Collections.Generic;
using System.IO;

partial class Util
{
    public static DirectoryInfo Combine(this DirectoryInfo directory, params string[] dir_names)
    {
        string[] temp = new string[1 + dir_names.Length];
        temp[0] = directory.FullName;
        for (int i = 0; i < dir_names.Length; i++)
            temp[1 + i] = dir_names[i];
        string combine = Path.Combine(temp);
        return new(combine);
    }
}