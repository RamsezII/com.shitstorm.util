using System.IO;
using System.Text;
using UnityEngine;

partial class Util
{
    public static float ReadFloat(this string[] lines, ref int read_i) => float.Parse(lines[read_i++]);
    public static Vector3 ReadVector3(this string[] lines, ref int read_i) => new(ReadFloat(lines, ref read_i), ReadFloat(lines, ref read_i), ReadFloat(lines, ref read_i));
    public static void WriteVector3(this StreamWriter writer, in Vector3 value) => writer.WriteLine($"{value.x}\n{value.y}\n{value.z}");
}

namespace _UTIL_
{
    public class StdWriterText : StreamWriter
    {
        public StdWriterText(in string path, in bool append = false) : base(path, append, Encoding.UTF8)
        {

        }

        //----------------------------------------------------------------------------------------------------------

    }
}