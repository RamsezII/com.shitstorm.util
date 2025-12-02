using UnityEngine;
using UnityEngine.UI;

partial class Util
{
    public static void SetSprite(this Image image, in Sprite sprite, in bool toggle = true)
    {
        if (toggle)
            image.gameObject.SetActive(sprite != null);
        image.sprite = sprite;
        if (sprite != null)
            image.SetNativeSize();
    }
}