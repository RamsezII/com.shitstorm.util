#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor.PackageManager;

namespace _UTIL_.Editor
{
    partial class Util_e
    {
        public static PackageInfo FromType<T>()
        {
            Assembly assembly = typeof(T).Assembly;

            PackageInfo package = PackageInfo.FindForAssembly(assembly)
                ?? throw new InvalidOperationException($"Le type {typeof(T).FullName} appartient à l'assembly '{assembly.GetName().FullName}', mais cette assembly ne semble pas provenir d'un package UPM.");

            return package;
        }

        public static string GetAbsolutePath<T>(in string relativePath = null)
        {
            PackageInfo package = FromType<T>();

            string path = package.resolvedPath;

            path = Path.Combine(path, relativePath);

            return Path.GetFullPath(path);
        }
    }
}
#endif