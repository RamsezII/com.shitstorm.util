using UnityEngine;

public static partial class Util
{
    public static Color ModifyHsv(this Color color, in float hue, float alpha)
    {
        Color.RGBToHSV(color, out _, out float s, out float v);
        color = Color.HSVToRGB(hue, s, v);
        color.a = alpha;
        return color;
    }

    public static Color WithAlpha(this Color color, in float alpha) => new(color.r, color.g, color.b, alpha);
}