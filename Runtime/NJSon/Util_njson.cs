using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEngine;

public static partial class Util_njson
{
    public static void NJSave(this JToken njson, in string path, in bool log = true)
    {
        DirectoryInfo pdir = Directory.GetParent(path);
        if (!pdir.Exists)
            pdir.Create();

        string text = JsonConvert.SerializeObject(njson, Formatting.Indented);
        File.WriteAllText(path, text);

        if (log)
            Debug.Log($"NJSave({path})".ToSubLog());
    }

    public static bool NJRead<T>(this string path, out T njson, in bool log_success = true, in bool log_failure = true) where T : JToken, new()
    {
        if (File.Exists(path))
        {
            try
            {
                string text = File.ReadAllText(path);
                njson = JsonConvert.DeserializeObject<T>(text);
                if (log_success)
                    Debug.Log($"{nameof(NJRead)}({path})".ToSubLog());
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"ERROR {nameof(NJRead)}: \"{e.TrimmedExceptionMessage()}\" ({path})");
                njson = new();
                return false;
            }
        }
        else
        {
            if (log_failure)
                Debug.LogWarning($"{nameof(NJRead)} no NJSon at: \"{path}\"");
            njson = new();
            return false;
        }
    }
}