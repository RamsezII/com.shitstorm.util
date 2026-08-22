using System;
using System.Text;
using UnityEngine;

partial class Util
{
    public static void Log(this StringBuilder sb, in UnityEngine.Object o = null) => Debug.Log(TroncatedForLog(sb), o);
    public static string TroncatedForLog(this StringBuilder sb)
    {
        if (sb == null || sb.Length == 0)
            return string.Empty;

        string text = sb.ToString();

        return text.EndsWith(Environment.NewLine, StringComparison.Ordinal)
            ? text[..^Environment.NewLine.Length]
            : text;
    }
}