using UnityEngine;

partial class Util
{
    // Garde target entièrement dans container (rt_screen), padding en unités UI
    public static bool GetStayInsideCorrection(this RectTransform target, in RectTransform container, in Vector2 padding, out Vector2 worldDelta)
    {
        // Corners du target en world
        var w = new Vector3[4];
        target.GetWorldCorners(w);

        // Convertit les corners dans l'espace local du container
        Vector2 min = new(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new(float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < 4; i++)
        {
            Vector3 p = container.InverseTransformPoint(w[i]);
            min = Vector2.Min(min, (Vector2)p);
            max = Vector2.Max(max, (Vector2)p);
        }

        Rect cr = container.rect;

        // On veut que min>=cr.min+padding et max<=cr.max-padding
        Vector2 offset = Vector2.zero;

        float left = cr.xMin + padding.x - min.x;
        float right = cr.xMax - padding.x - max.x;
        float bottom = cr.yMin + padding.y - min.y;
        float top = cr.yMax - padding.y - max.y;

        if (left > 0)
            offset.x += left;
        if (right < 0)
            offset.x += right;

        if (bottom > 0)
            offset.y += bottom;
        if (top < 0)
            offset.y += top;

        // Applique l'offset dans l'espace du container (local), converti en world delta
        // (comme container et target sont dans le même Canvas, ça marche bien)
        worldDelta = container.TransformVector((Vector3)offset);

        return worldDelta.sqrMagnitude > float.Epsilon;
    }

    public static Rect GetWorldRect(this RectTransform rt)
    {
        var w = new Vector3[4];
        rt.GetWorldCorners(w);

        Vector2 min = w[0];
        Vector2 max = w[0];

        for (int i = 1; i < 4; i++)
        {
            min = Vector2.Min(min, w[i]);
            max = Vector2.Max(max, w[i]);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    public static Rect GetWorldRect_fast(in RectTransform rt)
    {
        Vector3 pos = rt.TransformPoint(rt.rect.position);
        Vector2 size = Vector2.Scale(rt.rect.size, rt.lossyScale);
        return new Rect(pos, size);
    }

    public static Rect GetRectInParent(this RectTransform target, in RectTransform parent)
    {
        var w = new Vector3[4];
        target.GetWorldCorners(w);

        Vector2 min = new(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new(float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < 4; i++)
        {
            Vector3 p = parent.InverseTransformPoint(w[i]);
            min = Vector2.Min(min, (Vector2)p);
            max = Vector2.Max(max, (Vector2)p);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

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