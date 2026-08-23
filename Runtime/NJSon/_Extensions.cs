using System;

partial class Util
{
    public static string GetJSonExtension(this Type type) => $".{type}.json.txt";
    public static string GetJSonExtension_noTXT(this Type type) => $".{type}.json";
    public static string GetJSonFileName(this Type type) => $"{type}.json.txt";
    public static string GetJSonFileName_noTXT(this Type type) => $"{type}.json";
}