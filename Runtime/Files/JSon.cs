using System;
using System.IO;
using UnityEngine;

public abstract class JSon
{
    public const string
        txt = ".txt",
        json = ".json" + txt;

    //----------------------------------------------------------------------------------------------------------

    public static string GetJSonName<T>() => GetJSonName(typeof(T));
    public static string GetJSonName(in Type type) => type.FullName + json;
    public static string GetJSonExtension<T>() => GetJSonExtension(typeof(T));
    public static string GetJSonExtension(in Type type) => "." + GetJSonName(type);

    public void Save(in string filepath, in bool log) => Save(filepath, JsonUtility.ToJson(this, prettyPrint: true), log);
    public static void Save(in string filepath, in string text, in bool log)
    {
        filepath.CheckParentDirectory();

        if (File.Exists(filepath))
            File.SetAttributes(filepath, FileAttributes.Normal);

        File.WriteAllText(filepath, text);
        File.SetAttributes(filepath, FileAttributes.Normal);

        if (log)
            Debug.Log($"saved : \"{filepath}\"".ToSubLog());
    }

    //----------------------------------------------------------------------------------------------------------

    public static bool Read<T>(ref T json, in string filepath, in bool force, in bool log) where T : JSon, new()
    {
        json ??= new T();
        if (File.Exists(filepath))
        {
            string text = File.ReadAllText(filepath);

            try
            {
                json = JsonUtility.FromJson<T>(text);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"READ_JSON_ERROR: \"{e.TrimmedExceptionMessage()}\" ({filepath})");
                json = new();
                json.Save(filepath, true);
            }

            if (log)
                Debug.Log($"read: \"{filepath}\"".ToSubLog());

            return true;
        }
        else
        {
            if (force)
            {
                json.Save(filepath, true);
                return false;
            }
            else
                Debug.LogWarning($"no json at: \"{filepath}\"");
            return false;
        }
    }
}