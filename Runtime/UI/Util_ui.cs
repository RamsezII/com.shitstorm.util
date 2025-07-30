using UnityEngine;
using UnityEngine.UI;

partial class Util
{
    static readonly Vector3[] rt_corners = new Vector3[4];

    //--------------------------------------------------------------------------------------------------------------

    public static void SetSprite(this Image image, in Sprite sprite, in bool toggle = true)
    {
        if (toggle)
            image.gameObject.SetActive(sprite != null);
        image.sprite = sprite;
        if (sprite != null)
            image.SetNativeSize();
    }

    public static (Vector2 min, Vector2 max) GetWorldCorners(this RectTransform rT)
    {
        lock (rt_corners)
        {
            rT.GetWorldCorners(rt_corners);
            return (rt_corners[0], rt_corners[2]);
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
}