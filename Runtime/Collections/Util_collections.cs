using System.Collections.Generic;

partial class Util
{
    public static T PopLast<T>(this List<T> list)
    {
        T last = list[^1];
        list.RemoveAt(list.Count - 1);
        return last;
    }

    public static IEnumerable<T> EParams<T>(params T[] values)
    {
        foreach (var value in values)
            yield return value;
    }
}