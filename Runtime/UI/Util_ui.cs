using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

partial class Util
{
    static readonly Vector3[] rt_corners = new Vector3[4];

    //--------------------------------------------------------------------------------------------------------------

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

    public static void SetSprite(this Image image, in Sprite sprite, in bool toggle = true)
    {
        if (toggle)
            image.gameObject.SetActive(sprite != null);
        image.sprite = sprite;
        if (sprite != null)
            image.SetNativeSize();
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

    public static string Get_ItemName_From_DropdownToggle(this Toggle toggle)
    {
        string name = toggle.name;
        int index = name.IndexOf(':');
        if (index == -1)
            return name;
        return name[(index + 2)..];
    }

    public static bool IsInputFieldFocused()
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        if (obj == null)
            return false;
        return obj.GetComponent<TMP_InputField>() != null;
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
        return inside;
    }
}