using System;

partial class Util
{    
    public static U ToEnum<U>(this int value) where U : Enum => (U)Enum.ToObject(typeof(U), value);
    public static U CastEnum<T, U>(this T value) where T : Enum where U : Enum => ToEnum<U>(Convert.ToInt32(value));
}