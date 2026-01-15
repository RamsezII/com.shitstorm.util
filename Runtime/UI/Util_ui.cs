using UnityEngine;

partial class Util
{
    static readonly Vector3[] rt_corners = new Vector3[4];

    //--------------------------------------------------------------------------------------------------------------

    public static Vector2 InverseLerp(this in Rect rect, in Vector2 point)
    {
        float u = InverseLerpUnclamped(rect.xMin, rect.xMax, point.x);
        float v = InverseLerpUnclamped(rect.yMin, rect.yMax, point.y);
        return new Vector2(u, v);
    }

    public static bool IsNotInside(in Rect r_child, in Rect r_parent, out Vector2 error)
    {
        error = Vector2.zero;

        error.x = Mathf.Clamp(
            value: 0,
            min: Mathf.Max(0, r_parent.xMin - r_child.xMin),
            max: Mathf.Min(0, r_parent.xMax - r_child.xMax)
        );

        error.y = Mathf.Clamp(
            value: 0,
            min: Mathf.Max(0, r_parent.yMin - r_child.yMin),
            max: Mathf.Min(0, r_parent.yMax - r_child.yMax)
        );

        return error.sqrMagnitude > 0;
    }

    public static Vector2 WorldToLocalPosition(this RectTransform rt, in Vector3 worldPos, in Canvas canvas)
    {
        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null
            : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt,
            RectTransformUtility.WorldToScreenPoint(cam, worldPos),
            cam,
            out Vector2 lp);

        return lp;
    }

    public static void GetWorldCorners(this RectTransform rT, out Vector2 min, out Vector2 max)
    {
        lock (rt_corners)
        {
            rT.GetWorldCorners(rt_corners);
            min = rt_corners[0];
            max = rt_corners[2];
        }
    }

    public static void GetWorldCorners(this RectTransform rT, out Vector3 min, out Vector3 max)
    {
        lock (rt_corners)
        {
            rT.GetWorldCorners(rt_corners);
            min = rt_corners[0];
            max = rt_corners[2];
        }
    }

    public static bool BoundsClamp(
        in Vector2 current_min, in Vector2 current_max,
        in Vector2 parent_min, in Vector2 parent_max,
        out Vector2 correction)
    {
        correction = Vector2.zero;

        for (int i = 0; i < 2; ++i)
        {
            float _cmin = current_min[i];
            float _cmax = current_max[i];
            float _pmin = parent_min[i];
            float _pmax = parent_max[i];

            if (_cmin < _pmin)
                correction[i] -= _cmin - _pmin;

            if (_cmax > _pmax)
                correction[i] -= _cmax - _pmax;
        }

        return correction != Vector2.zero;
    }

    public static bool BoundsClamp(
        in Vector3 current_min, in Vector3 current_max,
        in Vector3 parent_min, in Vector3 parent_max,
        out Vector3 correction)
    {
        correction = Vector3.zero;

        for (int i = 0; i < 3; ++i)
        {
            float _cmin = current_min[i];
            float _cmax = current_max[i];
            float _pmin = parent_min[i];
            float _pmax = parent_max[i];

            if (_cmin < _pmin)
                correction[i] -= _cmin - _pmin;

            if (_cmax > _pmax)
                correction[i] -= _cmax - _pmax;
        }

        return correction != Vector3.zero;
    }

    /// <summary>
    /// Renvoie la position de la souris normalisée (0..1, 0..1) dans le RectTransform.
    /// (0,0) = bas-gauche du rect, (1,1) = haut-droite.
    /// Peut sortir [0,1] si la souris est hors du rect.
    /// </summary>
    public static bool GetMouseUVInRect(this RectTransform rect, in Vector2 mousePos, in Camera uiCamera, out Vector2 uv)
    {
        // 1) Écran → local dans le rect
        bool inside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            mousePos,
            uiCamera,
            out Vector2 local
        );

        // 2) On normalise dans le rect
        // rect.rect.x/y = coin bas-gauche du rect dans l'espace local
        Rect r = rect.rect;

        float u = (local.x - r.x) / r.width;
        float v = (local.y - r.y) / r.height;

        uv = new Vector2(u, v);

        return u >= 0 && u <= 1 && v >= 0 && v <= 1;
    }
}