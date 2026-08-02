using UnityEngine;

partial class Util
{
    public static Vector3 TransformPoint_unscaled(this Transform transform, in Vector3 position) => transform.position + transform.rotation * position;
    public static Vector3 InverseTransformPoint_unscaled(this Transform transform, in Vector3 position) => Quaternion.Inverse(transform.rotation) * (position - transform.position);

    public static void FillParent(this RectTransform rT)
    {
        rT.anchorMin = Vector2.zero;
        rT.anchorMax = Vector2.one;
        rT.sizeDelta = Vector2.zero;
        rT.anchoredPosition3D = Vector3.zero;
        rT.localScale = Vector3.one;
        rT.pivot = .5f * Vector2.one;
    }
}