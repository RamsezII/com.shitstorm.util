using System.IO;
using UnityEngine;

namespace _UTIL_
{
    public abstract class SettingsFile : JSon
    {
#if UNITY_EDITOR
        string GetSavePath() => Path.Combine(Path.Combine(Application.dataPath, "Resources").GetDir(true).FullName, GetJSonName(GetType()));
#endif
        string GetLoadPath() => GetJSonName(GetType());

        //--------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        public void Save() => Save(GetSavePath(), true);
#endif
        public static void Load<T>(ref T text, in bool log) where T : SettingsFile, new()
        {
            text = new();
            string resource_name = text.GetLoadPath()[..^4];
            TextAsset t = Resources.Load<TextAsset>(resource_name);

            if (t == null)
            {
                Debug.LogError($"No resource found at '{resource_name}'.");
#if UNITY_EDITOR
                text.Save();
#endif
            }
            else
                JsonUtility.FromJsonOverwrite(t.text, text);
        }
    }
}