#if UNITY_EDITOR
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
            if (Application.platform != RuntimePlatform.WindowsEditor &&
                Application.platform != RuntimePlatform.LinuxEditor)
            {
                Debug.LogError("Git Watch est actuellement disponible uniquement sous Windows et Linux.");
                return;
            }

            GitWatchLinuxWindow.Open(Directory.GetParent(Application.dataPath)!.FullName);
        }

        [MenuItem(MenuPath, true)]
        static bool ValidateStartGitWatch() =>
            Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.LinuxEditor;

    }
}
#endif
