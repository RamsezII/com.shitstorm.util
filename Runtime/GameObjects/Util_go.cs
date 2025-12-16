using UnityEngine;

partial class Util
{
    public static GameObject Clone(this GameObject prefab) => Object.Instantiate(prefab, prefab.transform.parent);
    public static T Clone<T>(this T prefab) where T : MonoBehaviour => Object.Instantiate(prefab, prefab.transform.parent);
}