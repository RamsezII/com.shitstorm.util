using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEngine;

partial class Util
{
    public static void NJSave(this JToken njson, in string path, in bool log = true, in Formatting formatting = Formatting.Indented)
    {
        DirectoryInfo pdir = Directory.GetParent(path);
        if (!pdir.Exists)
            pdir.Create();

        string text = JsonConvert.SerializeObject(njson, formatting);
        File.WriteAllText(path, text);

        if (log)
            Debug.Log($"{nameof(NJSave)}({path})".ToSubLog());
    }

    public static bool TryNJRead<T>(this string path, out T njson, in bool force = false, in bool log_success = true, in bool log_failure = true) where T : JToken, new()
    {
        njson = new();

        if (File.Exists(path))
        {
            try
            {
                string text = File.ReadAllText(path);
                njson = JsonConvert.DeserializeObject<T>(text);

                if (njson == null)
                {
                    if (log_failure)
                        Debug.LogWarning($"{nameof(TryNJRead)} empty NJSon at: \"{path}\"");
                    njson = new();
                    return false;
                }

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
            }

            return false;
        }
    }

    public static bool TryNJRead_resource<T>(this string rname, out T njson, in bool log_success = true, in bool log_failure = true) where T : JToken, new()
    {
        TextAsset rtext = Resources.Load<TextAsset>(rname);
        if (rtext == null)
            Debug.LogWarning($"{nameof(TryNJRead_resource)} no resource named: \"{rname}\"");
        else
        {
            njson = JsonConvert.DeserializeObject<T>(rtext.text);
            if (log_success)
                Debug.Log($"{nameof(TryNJRead_resource)}({rname})".ToSubLog());
            return true;
        }

        njson = null;
        return false;
    }
}