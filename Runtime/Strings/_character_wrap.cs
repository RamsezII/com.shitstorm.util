partial class Util
{
    public const char
        PROMPT_CHAR = '»',
        NOWRAP_CHAR = '\u00A0';

    //----------------------------------------------------------------------------------------------------------

    public static string AllSpaceToNowrap(this string text) => HasSpaces(text) ? text.Replace(' ', NOWRAP_CHAR) : text;
    public static string AllNowrapToSpace(this string text) => text.Contains(NOWRAP_CHAR) ? text.Replace(NOWRAP_CHAR, ' ') : text;

    public static bool HasSpaces(this string text) => text.Contains2("\t ");

    public static bool ForceUnwrappable(ref string text)
    {
        if (!HasSpaces(text))
            return true;
        text = text.Replace(' ', NOWRAP_CHAR);
        return false;
    }

    public static bool RemoveNowraps(ref string text)
    {
        if (text.Contains(NOWRAP_CHAR))
        {
            text = text.Remove(NOWRAP_CHAR, ' ');
            return true;
        }
        return false;
    }
}