using System.IO;
using UnityEngine;

partial class Util
{
    public static Color ReadColor_3f16(this BinaryReader reader) => new(reader.Read_f16(), reader.Read_f16(), reader.Read_f16());
    public static void WriteColor_3f16(this BinaryWriter writer, in Color color)
    {
        writer.Write_f16(color.r);
        writer.Write_f16(color.g);
        writer.Write_f16(color.b);
    }

    public static Color ReadColor_4f16(this BinaryReader reader) => new(reader.Read_f16(), reader.Read_f16(), reader.Read_f16(), reader.Read_f16());
    public static void WriteColor_4f16(this BinaryWriter writer, in Color color)
    {
        writer.Write_f16(color.r);
        writer.Write_f16(color.g);
        writer.Write_f16(color.b);
        writer.Write_f16(color.a);
    }

    public static Color ReadColor_3f32(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    public static void WriteColor_3f32(this BinaryWriter writer, in Color color)
    {
        writer.Write(color.r);
        writer.Write(color.g);
        writer.Write(color.b);
    }

    public static Color ReadColor_4f32(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    public static void WriteColor_4f32(this BinaryWriter writer, in Color color)
    {
        writer.Write(color.r);
        writer.Write(color.g);
        writer.Write(color.b);
        writer.Write(color.a);
    }
}