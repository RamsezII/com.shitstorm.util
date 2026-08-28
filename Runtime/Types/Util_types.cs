using System;
using System.IO;

partial class Util
{
    public static U ToEnum<U>(this int value) where U : Enum => (U)Enum.ToObject(typeof(U), value);
    public static U CastEnum<T, U>(this T value) where T : Enum where U : Enum => ToEnum<U>(Convert.ToInt32(value));

    public static void WriteType(this BinaryWriter writer, in Type type)
    {
        if (type == null)
            writer.Write(string.Empty);
        else
            writer.Write(type.FullName);
    }

    public static Type ReadType(this BinaryReader reader)
    {
        string typeName = reader.ReadString();
        if (string.IsNullOrEmpty(typeName))
            return CastType(typeName);
        return null;
    }
}