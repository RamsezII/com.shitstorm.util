using System;

partial class Util
{
    public static void SetFlags<T>(ref T mask, T flags, bool value) where T : Enum => mask = SetFlags_copy(mask, flags, value);
    public static T SetFlags_copy<T>(this T mask, T flags, bool value) where T : Enum
    {
        int _mask = Convert.ToInt32(mask);
        int _flags = Convert.ToInt32(flags);

        if (value)
            _mask |= _flags;
        else
            _mask &= ~_flags;

        return (T)Enum.ToObject(typeof(T), _mask);
    }
}