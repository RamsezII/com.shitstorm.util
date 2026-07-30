using System;
using System.Runtime.InteropServices;
using UnityEngine;

partial class Util
{
    public static bool IsAppWindows => Application.platform.ToString().Contains("Windows", StringComparison.OrdinalIgnoreCase);
    public static bool IsAppLinux => Application.platform.ToString().Contains("Linux", StringComparison.OrdinalIgnoreCase);
    public static bool IsOsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsOsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
}