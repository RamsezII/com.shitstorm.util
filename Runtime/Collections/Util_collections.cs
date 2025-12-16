using System;
using System.Collections.Generic;

partial class Util
{
    public static T PopLast<T>(this List<T> list)
    {
        T last = list[^1];
        list.RemoveAt(list.Count - 1);
        return last;
    }

    public static bool TryPeekLast<T>(this List<T> list, out T last)
    {
        if (list == null || list.Count == 0)
        {
            last = default;
            return false;
        }
        last = list[^1];
        return true;
    }

    public static bool TryPopLast<T>(this List<T> list, out T last)
    {
        if (TryPeekLast(list, out last))
        {
            list.RemoveAt(list.Count - 1);
            return true;
        }
        last = default;
        return false;
    }

    public static Dictionary<string, T> EnumToDict<T>(in IEqualityComparer<string> comparer) where T : struct, Enum
    {
        var type = typeof(T);
        var names = Enum.GetNames(type);
        var values = (T[])Enum.GetValues(type);

        var dict = new Dictionary<string, T>(names.Length, comparer);
        for (int i = 0; i < names.Length; i++)
            dict[names[i]] = values[i];

        return dict;
    }

    public static Dictionary<string, object> EnumToDict(in Type enumType, in IEqualityComparer<string> comparer)
    {
        if (enumType is null) throw new ArgumentNullException(nameof(enumType));
        if (!enumType.IsEnum)
            throw new ArgumentException($"expected enum, got {enumType.FullName}", nameof(enumType));

        var names = Enum.GetNames(enumType);
        var values = Enum.GetValues(enumType); // Array non-générique

        var dict = new Dictionary<string, object>(names.Length, comparer);
        for (int i = 0; i < names.Length; i++)
            dict[names[i]] = values.GetValue(i)!;

        return dict;
    }
}