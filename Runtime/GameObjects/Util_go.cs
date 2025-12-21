using UnityEngine;

partial class Util
{
    public static GameObject Clone(this GameObject prefab) => Object.Instantiate(prefab, prefab.transform.parent);
    public static GameObject Clone(this GameObject prefab, in bool set_active)
    {
        GameObject clone = Object.Instantiate(prefab, prefab.transform.parent);
        if (set_active)
            clone.SetActive(true);
        return clone;
    }

    public static T Clone<T>(this T prefab, in bool set_active = false, in Transform parent = null) where T : MonoBehaviour
    {
        T clone = Object.Instantiate(prefab, parent != null ? parent : prefab.transform.parent);
        if (set_active)
            clone.gameObject.SetActive(true);
        return clone;
    }
}