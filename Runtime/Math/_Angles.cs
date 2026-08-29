using UnityEngine;

public static partial class Util
{
    public static int ToRotationnalIndex(this Vector2 direction, in int count)
    {
        float angle = Vector2.SignedAngle(Vector2.up, direction);
        angle = (-angle + 360) % 360;
        int index = Mathf.RoundToInt(angle / (360f / count)) % count;
        return index;
    }
}