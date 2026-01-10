using System;
using System.IO;
using UnityEngine;

public abstract class JSon
{
    public const string
        txt = ".txt",
        json = ".json" + txt;

    //----------------------------------------------------------------------------------------------------------

    public static string GetJSonName<T>() where T : JSon => GetJSonName(typeof(T));
    public static string GetJSonName(in Type type) => type.FullName + json;
    public static string GetJSonExtension<T>() where T : JSon => GetJSonExtension(typeof(T));
    public static string GetJSonExtension(in Type type) => "." + GetJSonName(type);

    protected virtual void OnSave() { }
    public void Save(in string filepath, in bool log)
    {
        OnSave();
        Save(filepath, JsonUtility.ToJson(this, true), log);
    }

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

    public virtual void OnRead() => OnApply();
    protected virtual void OnApply() { }

    public virtual void WriteBytes(in BinaryWriter writer)
    {

    }

    public virtual void ReadBytes(in BinaryReader reader)
    {

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

            json.OnRead();

            if (log)
                Debug.Log($"read: \"{filepath}\"".ToSubLog());

            return true;
        }
        else
        {
            if (force)
            {
                json.Save(filepath, true);
                json.OnRead();
                return false;
            }
            else
                Debug.LogWarning($"no json at: \"{filepath}\"");
            return false;
        }
    }
}