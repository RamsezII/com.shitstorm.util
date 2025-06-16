using UnityEngine;

public static partial class Util
{
    public static bool PullValue(this ref bool flag)
    {
        if (flag)
        {
            flag = false;
            return true;
        }
        return false;
    }

    public static bool Equals2<T>(this T a, in T b)
    {
        if (a == null)
            return b == null;
        return a.Equals(b);
    }

    public static void Destroy(this Object obj)
    {
        if (Application.isPlaying)
            Object.Destroy(obj);
        else
            Object.DestroyImmediate(obj);
    }
}