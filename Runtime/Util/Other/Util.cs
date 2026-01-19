using System;
using UnityEngine;

public static partial class Util
{
    public static void Destroy(this UnityEngine.Object obj)
    {
        if (Application.isPlaying)
            UnityEngine.Object.Destroy(obj);
        else
            UnityEngine.Object.DestroyImmediate(obj);
    }

    public static void CallAction<T>(this T self, in Action<T> action) => action(self);

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
}