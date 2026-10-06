using UnityEngine;

partial class Util
{
    public static Vector3 TransformPoint_unscaled(this Transform transform, in Vector3 position) => transform.position + transform.rotation * position;
    public static Vector3 InverseTransformPoint_unscaled(this Transform transform, in Vector3 position) => Quaternion.Inverse(transform.rotation) * (position - transform.position);

    public static RectTransform AsRTfm(this Transform tfm) => (RectTransform)tfm;
    public static void FillParent(this RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition3D = Vector3.zero;
        rt.localScale = Vector3.one;
        rt.pivot = .5f * Vector2.one;
    }

    public static void CopyPositionAndRotation(this Transform a, in Transform b) => a.SetPositionAndRotation(b.position, b.rotation);
}