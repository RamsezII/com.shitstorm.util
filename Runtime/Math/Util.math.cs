using UnityEngine;

public static partial class Util
{
    public static ushort LoopID(this ref byte id) => ++id == 0 ? ++id : id;
    public static ushort LoopID(this ref ushort id) => ++id == 0 ? ++id : id;

    public static float Clamp_ref(ref this float value, in float min, in float max) => Mathf.Clamp(value, min, max);
    public static float Clamp01_ref(ref this float value) => Mathf.Clamp01(value);

    public static Vector2 ClampMagnitude_ref(ref this Vector2 value, in float maxLength) => Vector2.ClampMagnitude(value, maxLength);

    [System.Obsolete]
    public static int Repeat(this int value, int max)
    {
        switch (max)
        {
            case 0:
                Debug.LogWarning(nameof(max) + ": " + max + " == 0");
                return 0;

            case 1:
                return 0;

            default:
                while (value < 0)
                    value += max;
                return value % max;
        }
    }
    public static float InverseLerpUnclamped(in float a, in float b, in float value) => a == b ? 0f : (value - a) / (b - a);
}