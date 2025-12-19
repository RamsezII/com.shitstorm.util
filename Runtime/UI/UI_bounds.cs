using UnityEngine;

partial class Util
{
    public static bool WorldBoundsToScreenRect(
        in Bounds bounds,
        in Camera camera,
        out Rect screenRect
    )
    {
        screenRect = default;

        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;

        Vector3[] corners =
        {
            c + new Vector3(+e.x, +e.y, +e.z),
            c + new Vector3(+e.x, +e.y, -e.z),
            c + new Vector3(+e.x, -e.y, +e.z),
            c + new Vector3(+e.x, -e.y, -e.z),
            c + new Vector3(-e.x, +e.y, +e.z),
            c + new Vector3(-e.x, +e.y, -e.z),
            c + new Vector3(-e.x, -e.y, +e.z),
            c + new Vector3(-e.x, -e.y, -e.z),
        };

        Vector2 min = new(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new(float.NegativeInfinity, float.NegativeInfinity);
        bool anyInFront = false;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 w = corners[i];
            Vector3 sp = camera.WorldToViewportPoint(w);

            if (sp.z <= 0)
                continue;

            anyInFront = true;
            min = Vector2.Min(min, sp);
            max = Vector2.Max(max, sp);
        }

        if (!anyInFront)
            return false;

        screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return true;
    }
}