using UnityEngine;

public static partial class Util
{
    // Exponential smoothing: timeConstant seconds absorbs ~63% of the gap to a constant target.
    // A non-positive deltaTime keeps the current value; a non-positive timeConstant snaps to the target.
    public static float ExpSmoothFactor(in float timeConstant, in float deltaTime) => deltaTime <= 0f ? 0f : timeConstant <= 0f ? 1f : 1f - Mathf.Exp(-deltaTime / timeConstant);

    public static float ExpSmooth(in float current, in float target, in float timeConstant, in float deltaTime) => Mathf.Lerp(current, target, ExpSmoothFactor(timeConstant, deltaTime));
    public static Vector2 ExpSmooth(in Vector2 current, in Vector2 target, in float timeConstant, in float deltaTime) => Vector2.Lerp(current, target, ExpSmoothFactor(timeConstant, deltaTime));
    public static Vector3 ExpSmooth(in Vector3 current, in Vector3 target, in float timeConstant, in float deltaTime) => Vector3.Lerp(current, target, ExpSmoothFactor(timeConstant, deltaTime));
    public static Vector4 ExpSmooth(in Vector4 current, in Vector4 target, in float timeConstant, in float deltaTime) => Vector4.Lerp(current, target, ExpSmoothFactor(timeConstant, deltaTime));
    public static Color ExpSmooth(in Color current, in Color target, in float timeConstant, in float deltaTime) => Color.Lerp(current, target, ExpSmoothFactor(timeConstant, deltaTime));
    public static Quaternion ExpSmooth(in Quaternion current, in Quaternion target, in float timeConstant, in float deltaTime) => Quaternion.Slerp(current, target, ExpSmoothFactor(timeConstant, deltaTime));
    public static float ExpSmoothAngle(in float current, in float target, in float timeConstant, in float deltaTime) => Mathf.LerpAngle(current, target, ExpSmoothFactor(timeConstant, deltaTime));

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
    public static float RemapUnclamped(this float value, in float a1, in float b1, in float a2, in float b2) => Mathf.LerpUnclamped(a2, b2, InverseLerpUnclamped(a1, b1, value));
    public static float Remap(this float input, in float input_min, in float input_max, in float output_min, in float output_max) => Mathf.Lerp(output_min, output_max, Mathf.InverseLerp(input_min, input_max, input));
}
