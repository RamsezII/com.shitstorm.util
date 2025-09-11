using System.IO;
using UnityEngine;

namespace _UTIL_
{
    public abstract class SettingsFile : JSon
    {
        public string GetFilePath() => Path.Combine(Path.Combine(Application.dataPath, "Resources").GetDir(true).FullName, GetJSonName(GetType()));

        //--------------------------------------------------------------------------------------------------------------

        public void Save() => Save(GetFilePath(), true);
        public static void Load<T>(ref T text, in bool log) where T : SettingsFile, new()
        {
            text = new();
            Read(ref text, text.GetFilePath(), true, log);
        }
    }
}