using System.Text.RegularExpressions;

partial class Util
{
    public static string GlobToRegex(this string pattern)
    {
        var regex = Regex.Escape(pattern)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".");
        return "^" + regex + "$";
    }
}