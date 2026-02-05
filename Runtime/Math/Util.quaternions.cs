using UnityEngine;

public static partial class Util
{
    // Y, X, Z
    public static void Check(this ref Quaternion rot)
    {
        if (rot == new Quaternion())
            rot = Quaternion.identity;
    }

    public static Quaternion AtoB(this in Quaternion A, in Quaternion B) => B * Quaternion.Inverse(A);

    public static Vector4 ToVec4(this in Quaternion q) => new(q.x, q.y, q.z, q.w);

    public static Vector3 AngularVelocity(this in Quaternion A, in Quaternion B)
    {
        Quaternion deltaRotation = B * Quaternion.Inverse(A);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f)
            angle -= 360f;
        return angle * axis;
    }

    public static void FromToRotationAxisAngle(this in Quaternion A, in Quaternion B, out Vector3 axis, out float angle)
    {
        Quaternion deltaRotation = B * Quaternion.Inverse(A);
        deltaRotation.ToAngleAxis(out angle, out axis);
    }

    public static Quaternion RotateTowards(this in Quaternion rotation, in Vector3 from, in Vector3 to) => Quaternion.FromToRotation(rotation * from, to) * rotation;
}