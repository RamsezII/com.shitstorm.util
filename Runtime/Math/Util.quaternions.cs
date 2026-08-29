using UnityEngine;

public static partial class Util
{
    // Y, X, Z
    public static void Check(this ref Quaternion rot)
    {
        if (rot == new Quaternion())
            rot = Quaternion.identity;
    }

    public static void FromToRotationAxisAngle(this in Quaternion A, in Quaternion B, out Vector3 axis, out float angle)
    {
        Quaternion deltaRotation = B * Quaternion.Inverse(A);
        deltaRotation.ToAngleAxis(out angle, out axis);
    }

    public static Quaternion RotateTowards(this in Quaternion rotation, in Vector3 from, in Vector3 to) => Quaternion.FromToRotation(rotation * from, to) * rotation;
}