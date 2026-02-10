using System.IO;
using UnityEngine;

partial class Util
{
    public static void WriteAnimLayerWeight(this BinaryWriter writer, in Animator animator, in int layerIndex) => writer.Write_f16(animator.GetLayerWeight(layerIndex));
    public static void ReadAnimLayerWeight(this BinaryReader reader, in Animator animator, in int layerIndex) => animator.SetLayerWeight(layerIndex, reader.Read_f16());

    public static void WriteAnimFloat(this BinaryWriter writer, in Animator animator, in int parameter) => writer.Write_f16(animator.GetFloat(parameter));
    public static void ReadAnimFloat(this BinaryReader reader, in Animator animator, in int parameter) => animator.SetFloat(parameter, reader.Read_f16());
}