using System;
using System.Runtime.InteropServices;
using UnityEngine;

partial class Util
{
    public static readonly bool is_app_windows = Application.platform.ToString().Contains("Windows", StringComparison.OrdinalIgnoreCase);
    public static readonly bool is_app_linux = Application.platform.ToString().Contains("Linux", StringComparison.OrdinalIgnoreCase);
    public static readonly bool is_os_windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static readonly bool is_os_linux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
}