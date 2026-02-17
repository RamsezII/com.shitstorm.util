using UnityEngine;

public static partial class Util
{
    public static Vector3 SignedEuler_OLD(this Vector3 value)
    {
        for (int i = 0; i < 3; i++)
        {
            if (value[i] > 180)
                value[i] -= 360;
            else if (value[i] < -180)
                value[i] += 360;
        }
        return value;
    }

    public static Vector3 GetAbsoluteValue(this in Vector3 value) => new(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
 }