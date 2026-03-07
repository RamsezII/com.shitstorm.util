using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

public static partial class Util
{
    public static byte[] GetBuffer(this BinaryWriter writer) => ((MemoryStream)writer.BaseStream).GetBuffer();

    public static bool LengthPrefixedWrite(this BinaryWriter writer, in Action<BinaryWriter> onWriter)
    {
        ushort streamLength = (ushort)writer.BaseStream.Length;
        ushort startPos = (ushort)writer.BaseStream.Position;
        writer.Write((ushort)0);

        onWriter(writer);

        ushort endPos = (ushort)writer.BaseStream.Position;
        ushort dataSize = (ushort)(endPos - startPos - sizeof(ushort));

        if (dataSize == 0)
        {
            writer.BaseStream.Position = startPos;
            writer.BaseStream.SetLength(streamLength);
            return false;
        }
        else
        {
            writer.BaseStream.Position = startPos;
            writer.Write(dataSize);
            writer.BaseStream.Position = endPos;
            return true;
        }
    }

    public static void StartPrefixedWrite(this BinaryWriter writer, out ushort prefixePos)
    {
        prefixePos = (ushort)writer.BaseStream.Position;
        writer.Write((ushort)0);
    }

    public static ushort EndPrefixedWrite(this BinaryWriter writer, in ushort prefixePos)
    {
        ushort endPos = (ushort)writer.BaseStream.Position;
        writer.BaseStream.Position = prefixePos;
        ushort length = (ushort)(endPos - prefixePos - sizeof(ushort));
        writer.Write(length);
        writer.BaseStream.Position = endPos;
        return length;
    }

    public static void WriteString(this BinaryWriter writer, in string text)
    {
        if (string.IsNullOrEmpty(text))
            writer.Write(string.Empty);
        else
            writer.Write(text);
    }

    public static void WriteText(this BinaryWriter writer, in string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            writer.Write((ushort)0);
        else
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            writer.Write((ushort)bytes.Length);
            writer.BaseStream.Write(bytes);
        }
    }

    public static void Write(this BinaryWriter writer, in IPEndPoint netEnd)
    {
        if (netEnd == null)
        {
            Debug.LogWarning("trying to write null netEnd");
            writer.Write(new IPEndPoint(IPAddress.None, 0));
        }
        else
        {
            writer.Write((uint)netEnd.Address.Address);
            writer.Write((ushort)netEnd.Port);
        }
    }

    public static void Write_f16(this BinaryWriter writer, in float value) => writer.Write(Mathf.FloatToHalf(value));

    public static void WriteV3_3f32(this BinaryWriter writer, in Vector3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }

    public static void WriteV2_2f16(this BinaryWriter writer, in Vector2 value)
    {
        writer.Write_f16(value.x);
        writer.Write_f16(value.y);
    }

    public static void WriteV3_3f16(this BinaryWriter writer, in Vector3 value)
    {
        writer.Write_f16(value.x);
        writer.Write_f16(value.y);
        writer.Write_f16(value.z);
    }

    public static void WriteQ_3u8(this BinaryWriter writer, in Quaternion value)
    {
        Vector3 eul = byte.MaxValue / 360f * value.eulerAngles;
        writer.Write((byte)eul.x);
        writer.Write((byte)eul.y);
        writer.Write((byte)eul.z);
    }

    public static void WriteQ_3f16(this BinaryWriter writer, in Quaternion value)
    {
        Vector3 eulers = value.eulerAngles;
        for (int i = 0; i < 3; ++i)
            writer.Write_f16(eulers[i]);
    }

    public static void WriteQ_4f16(this BinaryWriter writer, in Quaternion value)
    {
        writer.Write_f16(value.x);
        writer.Write_f16(value.y);
        writer.Write_f16(value.z);
        writer.Write_f16(value.w);
    }

    public static void WriteQ_4f32(this BinaryWriter writer, in Quaternion value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
        writer.Write(value.w);
    }
}