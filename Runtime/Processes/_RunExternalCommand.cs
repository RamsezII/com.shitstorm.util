using System;
using System.Runtime.InteropServices;
using UnityEngine;

partial class Util
{
    public static readonly bool is_windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static readonly bool is_linux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static readonly bool is_mac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    //--------------------------------------------------------------------------------------------------------------

    public static void RunExternalCommand(in string work_dir, in string command_line, in bool log = true, Action<string> on_stdout = null, Action<string> on_err = null)
    {
        on_stdout ??= static stdout => Debug.Log(stdout);
        on_err ??= static error => Debug.LogWarning(error);

        if (log)
            on_stdout($"[CMD_start] {work_dir}$ {command_line}");

        using var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = is_windows ? "powershell" : "/bin/bash";
        process.StartInfo.Arguments = is_windows ? $"/C {command_line}" : $"-c \"{command_line}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = work_dir;

        process.Start();

        string stdout = process.StandardOutput.ReadToEnd();
        string err = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(stdout))
        {
            TrimNewline(ref stdout);
            on_stdout(stdout);
        }

        if (!string.IsNullOrWhiteSpace(err))
        {
            TrimNewline(ref err);
            on_err(err);
        }

        if (log)
            on_stdout("[CMD_end]");
    }
}