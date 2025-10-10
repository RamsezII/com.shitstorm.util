using UnityEngine;

partial class Util
{
    public const float rpm_to_degPerSec = 360f / 60f;

    //----------------------------------------------------------------------------------------------------------

    public static float GetMeterPerSeconds(this WheelCollider wheel) => ToMeterPerSecond(wheel.rpm, wheel.radius);
    public static float ToMeterPerSecond(this in float rpm, in float radius) => rpm * radius * 2 * Mathf.PI * 60;
    public static float ToRotationPerMinute(this in float mps, in float radius) => mps / (Mathf.PI * radius * 30);

    public static float NormalizedHeight(this WheelCollider wheel)
    {
        wheel.GetWorldPose(out Vector3 pos, out _);
        Vector3 local = wheel.transform.InverseTransformPoint(pos);
        return local.y / -wheel.suspensionDistance;
    }
}