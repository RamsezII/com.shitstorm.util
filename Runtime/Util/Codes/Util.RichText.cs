public enum TextB : byte
{
    align,
    allcaps,
    alpha,
    /// <summary>b</summary>
    bold,
    /// <summary>cspace</summary>
    caracter_spacing,
    color,
    font,
    /// <summary>pos</summary>
    horizontal_position,
    indent,
    /// <summary>i</summary>
    italic,
    /// <summary>line-height</summary>
    line_height,
    /// <summary>line-indent</summary>
    line_indent,
    link,
    lowercase,
    margin,
    mark,
    /// <summary>mspace</summary>
    monospace,
    /// <summary>nobr</summary>
    non_breaking_spaces,
    noparse,
    /// <summary><page></summary>
    page_break,
    size,
    smallcaps,
    space,
    sprite,
    /// <summary>s</summary>
    strikethrough,
    style,
    /// <summary>sub</summary>
    subscript,
    /// <summary>sup</summary>
    superscript,
    /// <summary>u</summary>
    underline,
    uppercase,
    /// <summary>voffset</summary>
    vertical_offset,
    width,
    _last_
}

public static partial class Util_richtext
{
    public static readonly string[] rtextAttr = new string[(int)TextB._last_];
    static Util_richtext()
    {
        for (TextB tb = 0; tb < TextB._last_; ++tb)
            rtextAttr[(int)tb] = tb switch
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
                _ => "" + tb,
            };
    }
}