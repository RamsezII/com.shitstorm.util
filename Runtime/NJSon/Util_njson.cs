using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEngine;

partial class Util
{
    public static string GetJSonExtension<T>() => GetJSonExtension(typeof(T));
    public static string GetJSonExtension(this Type type) => $".{type}.json.txt";
    public static string GetJSonFileName<T>() => GetJSonFileName(typeof(T));
    public static string GetJSonFileName(this Type type) => $"{type}.json.txt";
    public static void NJSave(this JToken njson, in string path, in bool log = true)
    {
        DirectoryInfo pdir = Directory.GetParent(path);
        if (!pdir.Exists)
            pdir.Create();

        string text = JsonConvert.SerializeObject(njson, Formatting.Indented);
        File.WriteAllText(path, text);

        if (log)
            Debug.Log($"{nameof(NJSave)}({path})".ToSubLog());
    }

    public static bool TryNJRead<T>(this string path, out T njson, in bool force = false, in bool log_success = true, in bool log_failure = true) where T : JToken, new()
    {
        njson = null;

        if (File.Exists(path))
        {
            try
            {
                string text = File.ReadAllText(path);
                njson = JsonConvert.DeserializeObject<T>(text);
                if (log_success)
                    Debug.Log($"{nameof(TryNJRead)}({path})".ToSubLog());
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"ERROR {nameof(TryNJRead)}: \"{e.TrimmedExceptionMessage()}\" ({path})");
                return false;
            }
        }
        else
        {
            if (log_failure)
                Debug.LogWarning($"{nameof(TryNJRead)} no NJSon at: \"{path}\"");

            if (force)
            {
                File.WriteAllText(path, string.Empty);
                Debug.Log($"{nameof(TryNJRead)} forced: \"{path}\"".ToSubLog());
                njson = new();
            }

            return false;
        }
    }
}