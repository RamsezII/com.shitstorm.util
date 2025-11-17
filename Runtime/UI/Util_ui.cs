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

    [Obsolete]
    public static (Vector2 min, Vector2 max) GetWorldCorners(this RectTransform rT)
    {
        lock (rt_corners)
        {
            rT.GetWorldCorners(rt_corners);
            return (rt_corners[0], rt_corners[2]);
        }
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

    public static Vector2 GetWorldSize(this RectTransform rT)
    {
        (Vector2 min, Vector2 max) = rT.GetWorldCorners();
        return max - min;
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
}