using System.Text;
using UnityEngine;

partial class Util
{
    public static string TroncatedForLog(this StringBuilder sb) => sb == null || sb.Length == 0 ? string.Empty : sb.ToString()[..^1];
    public static void Log(this StringBuilder sb, in Object o = null) => Debug.Log(TroncatedForLog(sb), o);
}