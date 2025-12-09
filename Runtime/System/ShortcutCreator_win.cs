using System;
using System.IO;
using System.Text;
using UnityEngine;

public static partial class Util_system
{
    public static string GetDesktopPath()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        // Essaye xdg-user-dir, sinon ~/Desktop (ou ~/Bureau selon locale)
        try {
            var psi = new System.Diagnostics.ProcessStartInfo("xdg-user-dir", "DESKTOP"){
                RedirectStandardOutput = true, UseShellExecute = false
            };
            var p = System.Diagnostics.Process.Start(psi);
            string s = p.StandardOutput.ReadToEnd().Trim(); p.WaitForExit();
            if (!string.IsNullOrEmpty(s) && Directory.Exists(s)) return s;
        } catch {}
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Desktop");
#else
        return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
#endif
    }

    public static bool CreateShortcut(in string path_to_shortcut, in string path_to_target, in string path_to_icon = null, in string args = null, in string workingDir = null)
    {
        try
        {
            StringBuilder sb = new();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            sb.AppendLine("[InternetShortcut]");
            sb.AppendLine($"URL=file:///{path_to_target}");

            if (!string.IsNullOrEmpty(path_to_icon))
                sb.AppendLine($"IconFile={path_to_icon}");
            else
                sb.AppendLine($"IconFile={path_to_target}");

            sb.AppendLine("IconIndex=0");

            if (!string.IsNullOrEmpty(workingDir))
                sb.AppendLine($"WorkingDirectory={workingDir}");

            if (!string.IsNullOrEmpty(args))
                sb.AppendLine($"Arguments={args}");

#else

            sb.AppendLine("[Desktop Entry]");
            sb.AppendLine("Version=1.0");
            sb.AppendLine("Type=Application");
            sb.AppendLine($"Name={Path.GetFileNameWithoutExtension(path_to_shortcut)}");
            sb.AppendLine($"Exec=\"{path_to_target}\" {(string.IsNullOrEmpty(args) ? "" : args)}");

            if (!string.IsNullOrEmpty(path_to_icon))
                sb.AppendLine($"Icon={path_to_icon}");

            sb.AppendLine("Terminal=false");

#endif
            File.WriteAllText(path_to_shortcut.EndsWith(".url", StringComparison.OrdinalIgnoreCase) ? path_to_shortcut : path_to_shortcut + ".url", sb.ToString());

            Debug.Log($"Shortcut created at: \"{path_to_shortcut}\" (\"{path_to_target}\")");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.LogWarning($"Failed to create shortcut at \"{path_to_shortcut}\"");
            return false;
        }
    }
}