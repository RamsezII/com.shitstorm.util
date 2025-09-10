using System.IO;
using UnityEngine;

partial class Util
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/" + nameof(_UTIL_) + "/" + nameof(OpenPlayerLog))]
#endif
    public static void OpenPlayerLog()
    {
        string logPath = GetPlayerLogPath();

        if (File.Exists(logPath))
            Application.OpenURL(logPath);
        else
            Debug.LogWarning($"Player.log not found at: {logPath}");
    }

    static string GetPlayerLogPath()
    {
#if UNITY_STANDALONE_WIN
        return Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "Low",
            Application.companyName,
            Application.productName,
            "Player.log"
        );
#else
        return Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
            ".config/unity3d",
            Application.companyName,
            Application.productName,
            "Player.log"
        );
#endif
    }
}