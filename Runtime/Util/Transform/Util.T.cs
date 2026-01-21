using System.Collections.Generic;
using UnityEngine;

public static partial class Util
{
    public static string GetPath(this Transform transform, in bool includeRoot)
    {
        string res = transform.name;

        while (transform.parent && (includeRoot || (transform.parent != transform.root)))
        {
            transform = transform.parent;
            res = transform.name + "/" + res;
        }

        return res;
    }

    public static string GetRelativePath(this Transform transform, in Transform root)
    {
        string res = transform.name;

        while (transform.parent && transform.parent != root)
        {
            transform = transform.parent;
            res = transform.name + "/" + res;
        }

        return res;
    }

    public static void NormalizeChildrenScales(this Transform transform)
    {
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
            t.localScale = Vector3.one;
    }

    public static void CleanAll(this Transform transform)
    {
        for (int i = 0; i < transform.childCount; ++i)
            Object.Destroy(transform.GetChild(i).gameObject);
    }

    public static Transform ForceFindTransform(this string path)
    {
        string[] splits = path.Split('/');
        Transform root;

        GameObject go = GameObject.Find(splits[0]);
        if (go == null)
            root = new GameObject(splits[0]).transform;
        else
            root = go.transform;

        if (splits.Length == 1)
            return root;
        else
            return ForceFind(root, splits[1..], false);
    }

    public static bool TryFind(this Transform root, in string path, out Transform transform)
    {
        transform = root.Find(path);
        if (transform != null)
            return true;
        transform = null;
        return false;
    }

    public static Transform ForceFind(this Transform root, in string path, in bool force_new) => ForceFind(root, path.Split('/'), force_new);
    public static Transform ForceFind(this Transform root, in IList<string> splits, in bool force_new)
    {
        Transform t1 = root;
        for (int i = 0; i < splits.Count - 1; ++i)
        {
            string branch = splits[i];
            Transform t2 = t1.Find(branch);
            if (t2 == null)
            {
                t2 = new GameObject(branch).transform;
                t2.SetParent(t1, false);
                t2.name = branch;
            }
            t1 = t2;
        }
        Transform t3 = t1.Find(splits[^1]);
        if (force_new || t3 == null)
        {
            t3 = new GameObject(splits[^1]).transform;
            t3.SetParent(t1, false);
        }
        return t3;
    }

    public static void DestroyAllByType<ComponentType>(this GameObject gameObject) where ComponentType : Component
    {
        foreach (ComponentType component in gameObject.GetComponentsInChildren<ComponentType>(true))
            UnityEngine.Object.Destroy(component.gameObject);
    }
}