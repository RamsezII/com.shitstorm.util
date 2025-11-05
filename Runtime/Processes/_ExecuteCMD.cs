using UnityEngine;

partial class Util
{
    public static void ExecuteCMD(in string working_dir, in string command)
    {
        GUIUtility.systemCopyBuffer = command;

        const Colors cmd_color = Colors.pink;

        Debug.Log($"{typeof(Util).FullName}.{nameof(ExecuteCMD).Italic()}:\n\t{nameof(working_dir)}: {working_dir},\n\t{nameof(command)}: {command},".SetColor(cmd_color));

        System.Diagnostics.Process process = new()
        {
            StartInfo = new()
            {
                FileName = is_windows ? "powershell.exe" : "/bin/bash",
                Arguments = is_windows ? $"/C {command}" : $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = working_dir
            }
        };

        try
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.Log($"{"[OUTPUT]".SetColor(Colors.white)} {e.Data}".SetColor(cmd_color));
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.LogWarning($"{"[ERROR]".SetColor(Colors.red)} {e.Data}".SetColor(cmd_color));
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            Debug.Log($"{"[EXIT_CODE]".SetColor(Colors.white)} {process.ExitCode}".SetColor(cmd_color));
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            process.Close();
        }
    }

    public static void ExecuteFile(in string working_dir, in string file_name, in string arguments)
    {
        const Colors cmd_color = Colors.pink;

        Debug.Log($"{typeof(Util).FullName}.{nameof(ExecuteCMD).Italic()}:\n\t{nameof(file_name)}: {file_name}\n\t{nameof(arguments)}: {arguments}".SetColor(cmd_color));

        System.Diagnostics.Process process = new()
        {
            StartInfo = new()
            {
                FileName = file_name,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = working_dir
            }
        };

        try
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.Log($"{"[OUTPUT]".SetColor(Colors.white)} {e.Data}".SetColor(cmd_color));
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.LogWarning($"{"[ERROR]".SetColor(Colors.red)} {e.Data}".SetColor(cmd_color));
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            Debug.Log($"{"[EXIT_CODE]".SetColor(Colors.white)} {process.ExitCode}".SetColor(cmd_color));
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            process.Close();
        }
    }
}