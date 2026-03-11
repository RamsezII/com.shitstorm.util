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
    public static string Message(this System.Exception e) => $"{e.GetType()} : \"{e.Message.TrimEnd('\n', '\r', '\t')}\"";
}