using System.IO;
using UnityEngine;

partial class Util
{
    public static void WriteAnimFloat(this BinaryWriter writer, in Animator animator, in int parameter) => writer.Write_f16(animator.GetFloat(parameter));
    public static void ReadAnimFloat(this BinaryReader reader, in Animator animator, in int parameter) => animator.SetFloat(parameter, reader.Read_f16());
}