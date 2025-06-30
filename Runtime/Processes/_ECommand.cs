using System;
using System.Threading.Tasks;
using UnityEngine;

partial class Util
{
    // Fonction bloquante mais streaming
    public static void RunExternalCommand_streaming(in string work_dir, in string command_line, in bool log = true, Action<string> on_stdout = null, Action<string> on_err = null, in Action on_end = null)
    {
        on_stdout ??= stdout => Debug.Log(stdout);
        on_err ??= stderr => Debug.LogWarning(stderr);

        if (log)
            on_stdout($"[CMD_start] {work_dir}$ {command_line}");

        using var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = IsWindows() ? "powershell" : "/bin/bash";
        process.StartInfo.Arguments = IsWindows() ? $"/C {command_line}" : $"-c \"{command_line}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = work_dir;

        process.Start();

        // On lit les deux flux en parallèle, pour éviter le deadlock
        var stdoutTask = Task.Run(() =>
        {
            string line;
            while ((line = process.StandardOutput.ReadLine()) != null)
            {
                TrimNewline(ref line);
                on_stdout(line);
            }
        });

        var stderrTask = Task.Run(() =>
        {
            string line;
            while ((line = process.StandardError.ReadLine()) != null)
            {
                TrimNewline(ref line);
                on_err(line);
            }
        });

        // Attend la fin du process
        process.WaitForExit();

        // Attend la fin des deux lectures (stdout, stderr)
        Task.WaitAll(stdoutTask, stderrTask);

        if (log)
            on_stdout("[CMD_end]");

        on_end?.Invoke();
    }
}
