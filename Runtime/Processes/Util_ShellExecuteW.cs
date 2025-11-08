using System;

partial class Util
{
#if UNITY_STANDALONE_WIN
    // https://learn.microsoft.com/windows/win32/api/shellapi/nf-shellapi-shellexecutew
    [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr ShellExecuteW(
        IntPtr hwnd,
        string lpOperation,   // "open" pour lancer
        string lpFile,        // chemin .exe / .bat / .lnk
        string lpParameters,  // arguments
        string lpDirectory,   // working dir
        int nShowCmd          // 1 = normal
    );

    public static void ShellExecute(string file, string args = "", string working_dir = "")
    {
        IntPtr result = ShellExecuteW(IntPtr.Zero, "open", file, args, working_dir, 1);
        long code = result.ToInt64();
        if (code <= 32)
        {
            throw new InvalidOperationException($"ShellExecuteW failed with code {code} when trying to open '{file}' with args '{args}' in dir '{working_dir}'.");
        }
    }
#endif

    [System.Runtime.InteropServices.DllImport("libc")]
    public static extern int system(string cmd);
}