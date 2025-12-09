using System;
using System.Text;

partial class Util
{
    public const char
        PROMPT_CHAR = '»',
        ZSPACE_CHAR = '\u200B'; // zero-width space

    //----------------------------------------------------------------------------------------------------------

    public static bool IsZSpaced(this string text)
    {
        for (int i = 0; i < text.Length; i += 2)
            if (text[i] == ZSPACE_CHAR)
                return true;
        return false;
    }

    public static bool UnZSpaced(this string text, out string unzspaced)
    {
        if (!text.Contains(ZSPACE_CHAR, StringComparison.OrdinalIgnoreCase))
        {
            unzspaced = text;
            return false;
        }

        StringBuilder sb = new();
        for (int i = 0; i < text.Length; i++)
            if (text[i] != ZSPACE_CHAR)
                sb.Append(text[i]);
        unzspaced = sb.ToString();

        return true;
    }

    public static bool ZSpaced(this string text, out string zspaced)
    {
        if (IsZSpaced(text))
        {
            zspaced = text;
            return false;
        }

        UnZSpaced(text, out string unzspaced);

        StringBuilder sb = new();
        for (int i = 0; i < unzspaced.Length; i++)
        {
            sb.Append(unzspaced[i]);
            sb.Append(ZSPACE_CHAR);
        }
        zspaced = sb.ToString();

        return !string.Equals(text, zspaced, StringComparison.Ordinal);
    }
}