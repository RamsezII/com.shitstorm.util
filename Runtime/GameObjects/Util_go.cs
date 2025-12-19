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

    public static T Clone<T>(this T prefab) where T : MonoBehaviour => Object.Instantiate(prefab, prefab.transform.parent);
    public static T Clone<T>(this T prefab, in bool set_active) where T : MonoBehaviour
    {
        T clone = Object.Instantiate(prefab, prefab.transform.parent);
        if (set_active)
            clone.gameObject.SetActive(true);
        return clone;
    }
}