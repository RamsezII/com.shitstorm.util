#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _UTIL_.Editor
{
    static class GitWatchMenu
    {
        const string MenuPath = "Assets/Git Watch";

        [MenuItem(MenuPath, false, 2000)]
        static void StartGitWatch()
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                Debug.LogError("Git Watch est actuellement disponible uniquement sous Windows.");
                return;
            }

            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
                string scriptPath = FindDashboardScript(projectRoot);

                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File {Quote(scriptPath)} -Root {Quote(projectRoot)}",
                    WorkingDirectory = projectRoot,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch (Exception exception)
            {
                Debug.LogError($"Impossible de lancer Git Watch :\n{exception}");
            }
        }

        [MenuItem(MenuPath, true)]
        static bool ValidateStartGitWatch() => Application.platform == RuntimePlatform.WindowsEditor;

        static string FindDashboardScript(string projectRoot)
        {
            foreach (string guid in AssetDatabase.FindAssets($"{nameof(GitWatchMenu)} t:MonoScript"))
            {
                string menuAssetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (!string.Equals(Path.GetFileName(menuAssetPath), $"{nameof(GitWatchMenu)}.cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                string menuDirectory = Path.GetDirectoryName(Path.Combine(projectRoot, menuAssetPath));
                string candidate = Path.GetFullPath(Path.Combine(menuDirectory!, "Binaries", "GitWatch.ps1"));

                if (File.Exists(candidate))
                    return candidate;
            }

            throw new FileNotFoundException("Le script Binaries/GitWatch.ps1 est introuvable dans le dépôt _UTIL_.");
        }

        static string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";
    }
}
#endif

