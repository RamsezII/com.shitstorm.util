#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace _UTIL_.Editor
{
    static class OpenTerminalHere
    {
        const string button_prefixe = "Assets/" + nameof(_UTIL_) + "/";

        //----------------------------------------------------------------------------------------------------------

        [MenuItem(button_prefixe + nameof(OpenTerminalHere), false)]
        static void OpenTerminal()
        {
            string projectPath = GetSelectedProjectPath();
            string absolutePath = ResolveAbsolutePath(projectPath);

            if (!Directory.Exists(absolutePath))
            {
                Debug.LogError($"Impossible d'ouvrir le terminal :\n{absolutePath}");
                return;
            }

            OpenTerminalAt(absolutePath);
        }

        [MenuItem(button_prefixe + nameof(OpenTerminalHere), true)]
        static bool ValidateOpenTerminal() => true;

        /// <summary>
        /// Retourne le dossier sélectionné dans la fenêtre Project.
        /// Si un fichier est sélectionné, retourne son dossier parent.
        /// </summary>
        static string GetSelectedProjectPath()
        {
            if (Selection.activeObject != null)
            {
                string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

                if (!string.IsNullOrWhiteSpace(selectedPath))
                {
                    if (AssetDatabase.IsValidFolder(selectedPath))
                        return NormalizePath(selectedPath);

                    string parentDirectory = Path.GetDirectoryName(selectedPath);

                    if (!string.IsNullOrWhiteSpace(parentDirectory))
                        return NormalizePath(parentDirectory);
                }
            }

            // Permet notamment de retrouver le dossier actuellement affiché
            // lorsqu'aucun asset précis n'est sélectionné.
            MethodInfo method = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (method?.Invoke(null, null) is string activeFolder && !string.IsNullOrWhiteSpace(activeFolder))
                return NormalizePath(activeFolder);

            return "Assets";
        }

        /// <summary>
        /// Convertit un chemin Unity, tel que Assets/... ou Packages/...,
        /// en chemin physique absolu.
        /// </summary>
        static string ResolveAbsolutePath(string projectPath)
        {
            projectPath = NormalizePath(projectPath);

            if (projectPath == "Packages" || projectPath.StartsWith("Packages/", StringComparison.Ordinal))
            {
                PackageInfo package = PackageInfo.FindForAssetPath(projectPath);

                if (package != null)
                {
                    string relativePath = projectPath[package.assetPath.Length..].TrimStart('/');
                    return Path.GetFullPath(Path.Combine(package.resolvedPath, relativePath));
                }
            }

            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;

            return Path.GetFullPath(Path.Combine(projectRoot, projectPath));
        }

        static void OpenTerminalAt(string directory)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    OpenWindowsTerminal(directory);
                    break;

                case RuntimePlatform.OSXEditor:
                    StartProcess("/usr/bin/open", $"-a Terminal {Quote(directory)}");
                    break;

                case RuntimePlatform.LinuxEditor:
                    OpenLinuxTerminal(directory);
                    break;

                default:
                    Debug.LogError(
                        $"Plateforme non prise en charge : {Application.platform}"
                    );
                    break;
            }
        }

        static void OpenWindowsTerminal(string directory)
        {
            // Essaie d'abord Windows Terminal.
            if (TryStartProcess("wt.exe", $"-d {Quote(directory)}"))
                return;

            // Fallback présent sur toutes les installations Windows.
            if (TryStartProcess("cmd.exe", $"/K cd /d {Quote(directory)}"))
                return;

            Debug.LogError("Aucun terminal Windows n'a pu être ouvert.");
        }

        static void OpenLinuxTerminal(string directory)
        {
            if (TryStartProcess("gnome-terminal", $"--working-directory={Quote(directory)}"))
                return;

            if (TryStartProcess("konsole", $"--workdir {Quote(directory)}"))
                return;

            if (TryStartProcess("xfce4-terminal", $"--working-directory={Quote(directory)}"))
                return;

            Debug.LogError("Aucun terminal Linux compatible n'a été trouvé.");
        }

        static bool TryStartProcess(string executable, string arguments)
        {
            try
            {
                StartProcess(executable, arguments);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static void StartProcess(string executable, string arguments)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                UseShellExecute = true
            });
        }

        static string Quote(this string value) => $"\"{value.Replace("\"", "\\\"")}\"";

        static string NormalizePath(this string path) => path.Replace('\\', '/');
    }
}
#endif