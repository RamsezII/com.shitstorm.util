using System;
using System.Collections.Generic;

partial class Util
{
    public static IEnumerable<T> EGetEnumValues<T>() where T : Enum
    {
        foreach (var o in Enum.GetValues(typeof(T)))
            yield return (T)o;
    }
}