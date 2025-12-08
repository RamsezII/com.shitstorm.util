using UnityEngine;

partial class Util
{
    public static Vector3 TransformPoint_unscaled(this Transform transform, in Vector3 position) => transform.position + transform.rotation * position;
    public static Vector3 InverseTransformPoint_unscaled(this Transform transform, in Vector3 position) => Quaternion.Inverse(transform.rotation) * (position - transform.position);
}