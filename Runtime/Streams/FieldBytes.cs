using System;
using System.IO;

namespace _UTIL_
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FieldBytesAttribute : Attribute
    {
    }
}

partial class Util
{
    public static void WriteFields<T>(this BinaryWriter writer, in object target, Type type = null) where T : Attribute
    {
        foreach (var field in (type ?? target.GetType()).EFields<T>())
            WriteFields(writer, field.GetValue(target));
    }

    public static void WriteFields(this BinaryWriter writer, object value)
    {
        var type = value.GetType();

        if (type.IsEnum)
        {
            type = Enum.GetUnderlyingType(type);
            value = Convert.ChangeType(value, type);
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean: writer.Write((bool)value); break;

            case TypeCode.Byte: writer.Write((byte)value); break;
            case TypeCode.SByte: writer.Write((sbyte)value); break;

            case TypeCode.Int16: writer.Write((short)value); break;
            case TypeCode.UInt16: writer.Write((ushort)value); break;

            case TypeCode.Int32: writer.Write((int)value); break;
            case TypeCode.UInt32: writer.Write((uint)value); break;

            case TypeCode.Int64: writer.Write((long)value); break;
            case TypeCode.UInt64: writer.Write((ulong)value); break;

            case TypeCode.Single: writer.Write((float)value); break;
            case TypeCode.Double: writer.Write((double)value); break;

            case TypeCode.String: writer.Write((string)value); break;

            default:
                throw new NotSupportedException(type.FullName);
        }
    }

    public static void ReadFields<T>(this BinaryReader reader, in object target, in Type type = null) where T : Attribute
    {
        foreach (var field in (type ?? target.GetType()).EFields<T>())
            field.SetValue(target, ReadFields(reader, field.FieldType));
    }

    public static object ReadFields(this BinaryReader reader, in Type type)
    {
        bool isEnum = type.IsEnum;
        Type serializedType = isEnum ? Enum.GetUnderlyingType(type) : type;

        object value = Type.GetTypeCode(serializedType) switch
        {
            TypeCode.Boolean => reader.ReadBoolean(),

            TypeCode.Byte => reader.ReadByte(),
            TypeCode.SByte => reader.ReadSByte(),

            TypeCode.Int16 => reader.ReadInt16(),
            TypeCode.UInt16 => reader.ReadUInt16(),

            TypeCode.Int32 => reader.ReadInt32(),
            TypeCode.UInt32 => reader.ReadUInt32(),

            TypeCode.Int64 => reader.ReadInt64(),
            TypeCode.UInt64 => reader.ReadUInt64(),

            TypeCode.Single => reader.ReadSingle(),
            TypeCode.Double => reader.ReadDouble(),

            TypeCode.String => reader.ReadString(),

            _ => throw new NotSupportedException(type.FullName)
        };

        return isEnum ? Enum.ToObject(type, value) : value;
    }
}