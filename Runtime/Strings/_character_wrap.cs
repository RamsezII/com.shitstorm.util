partial class Util
{
    public const char
        PROMPT_CHAR = '»',
        NOWRAP_CHAR = '\u00A0';

    //----------------------------------------------------------------------------------------------------------

    public static string ForceCharacterWrap(this string text) => HasSpaces(text) ? text.Replace(' ', NOWRAP_CHAR) : text;
    public static string ReplaceCharacterWraps(this string text) => text.Contains(NOWRAP_CHAR) ? text.Replace(NOWRAP_CHAR, ' ') : text;

    public static bool HasSpaces(this string text) => !string.IsNullOrEmpty(text) && text.Contains2("\t ");

    public static bool ForceCharacterWrap(ref string text)
    {
        if (!HasSpaces(text))
            return true;

        text = text.Replace(' ', NOWRAP_CHAR);
        return false;
    }

    public static bool RemoveCharacterWrap(ref string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        if (text.Contains(NOWRAP_CHAR))
        {
            text = text.Replace(NOWRAP_CHAR, ' ');
            return true;
        }

        return false;
    }
}