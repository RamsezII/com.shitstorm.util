public static partial class Util  // http://digitalnativestudios.com/textmeshpro/docs/rich-text/
{
    public static string Bold(this string text) => text.SetStyle(TextB.bold);
    public static string Italic(this string text) => text.SetStyle(TextB.italic);
    public static string SetStyle(this string text, in TextB style, string setting = "", in bool close = true, params string[] attributes)
    {
        foreach (string attr in attributes)
            setting += " " + attr;

        string name = RichText(style);

        if (close)
            return $"<{name}{setting}>{text}</{name}>";
        else
            return $"<{name}{setting}>{text}";
    }

    static string RichText(in TextB style) => style switch
    {
        TextB.line_height => "line-height",
        TextB.bold => "b",
        TextB.italic => "i",
        TextB.caracter_spacing => "cspace",
        TextB.line_indent => "line-indent",
        TextB.monospace => "mspace",
        TextB.non_breaking_spaces => "nobr",
        TextB.page_break => "<page>",
        TextB.horizontal_position => "pos",
        TextB.strikethrough => "s",
        TextB.underline => "u",
        TextB.subscript => "sub",
        TextB.superscript => "sup",
        TextB.vertical_offset => "voffset",
        _ => "" + style,
    };
}