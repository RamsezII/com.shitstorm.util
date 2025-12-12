using UnityEngine;

partial class Util
{
    public static T Clone<T>(this T prefab) where T : MonoBehaviour => Object.Instantiate(prefab, prefab.transform.parent);
}