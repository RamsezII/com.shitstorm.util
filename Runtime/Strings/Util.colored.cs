using UnityEngine;

public static partial class Util
{
    static string sublogColor = "#EEEEEE";

    //--------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad_LogColors()
    {
        if (UnityEditor.EditorGUIUtility.isProSkin)
            sublogColor = "#CCCCCC";
        Debug.Log($"{nameof(UnityEditor.EditorGUIUtility.isProSkin)}: {UnityEditor.EditorGUIUtility.isProSkin}".ToSubLog());
    }
#endif

    //--------------------------------------------------------------------------------------------------------------

    public static string SetSize_percent(this string text, in int percent) => $"<size={percent}%>{text}</size>";
    public static string SetColor(this string text, in Color color) => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>";
    public static string SetColor(this string text, in Colors color) => $"<color=#{(uint)color:X8}>{text}</color>";
    public static string SetColor(this string text, in string value) => $"<color={value}>{text}</color>";
    public static string ToSubLog(this object o) => ToSubLog(o.ToString());
    public static string ToSubLog(this string text) => text.SetAttribute(TextB.italic).SetAttribute(TextB.color, sublogColor);
    public static string TipToLog(this string tip) => $"<i><color=#DDDDDD>tip:</color> {tip}</i>";
    public static void LogAsTip(this string tip) => Debug.Log(TipToLog(tip));
    public static string Message(this System.Exception e) => $"{e.GetType()} : \"{e.Message.TrimEnd('\n', '\r', '\t')}\"";
    public static void Log(this object message, in Colors color, in Object context) => Debug.Log(message.ToString().SetColor(color), context);
}