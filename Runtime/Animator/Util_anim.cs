using System.IO;
using UnityEngine;

public static partial class Util
{
    public static int GetAnimatorHash(this string str) => Animator.StringToHash(str);

    public static float GetNormalizedTime(this Animator animator, in int layerIndex = 0) => animator.GetStateInfo_safe(layerIndex).normalizedTime;

    public static float GetNormalizedTimeClamped(this Animator animator, in int layerIndex = 0) => Mathf.Clamp01(GetNormalizedTime(animator, layerIndex));

    public static AnimatorStateInfo GetStateInfo_safe(this Animator animator, in int layerIndex) => animator.IsInTransition(layerIndex) ? animator.GetNextAnimatorStateInfo(layerIndex) : animator.GetCurrentAnimatorStateInfo(layerIndex);

    public static Transform GetRootBone(this Animator animator)
    {
        Transform root = animator.GetBoneTransform(HumanBodyBones.Hips);
        if (root != animator.transform)
            do { root = root.parent; }
            while (root != animator.transform);

        return root;
    }

    public static void WriteAllParameters(this BinaryWriter writer, in Animator animator)
    {
        if (animator != null)
            for (int i = 0; i < animator.parameterCount; ++i)
            {
                var param = animator.parameters[i];
                int hash = param.nameHash;

                switch (param.type)
                {
                    case AnimatorControllerParameterType.Float:
                        writer.Write_f16(animator.GetFloat(hash));
                        break;
                    case AnimatorControllerParameterType.Int:
                        writer.Write((byte)animator.GetInteger(hash));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        writer.Write(animator.GetBool(hash));
                        break;
                }
            }
    }

    public static void ReadAllParameters(this BinaryReader reader, in Animator animator)
    {
        if (animator != null)
            for (int i = 0; i < animator.parameterCount; ++i)
            {
                var param = animator.parameters[i];
                int hash = param.nameHash;

                switch (param.type)
                {
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(hash, reader.Read_f16());
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(hash, reader.ReadByte());
                        break;
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(hash, reader.ReadBoolean());
                        break;
                }
            }
    }
}