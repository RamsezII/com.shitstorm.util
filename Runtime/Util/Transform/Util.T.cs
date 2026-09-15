using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (root == null)
            throw new System.ArgumentNullException(paramName: nameof(root));

        string res = transform.name;

        while (transform.parent != null && transform.parent != root)
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

    public static bool TryFind(this Transform root, in string path, out Transform transform)
    {
        transform = root.Find(path);
        if (transform != null)
            return true;
        transform = null;
        return false;
    }

    public static bool TryFindRootTransform(in string name, out Transform tfm)
    {
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            if (go.name.Equals(name, System.StringComparison.Ordinal))
            {
                tfm = go.transform;
                return true;
            }
        tfm = null;
        return false;
    }

    public static Transform ForceFind(this Transform root, in string path, in bool clean = false)
    {
        string[] branches = path.Split('/');
        Transform current = root;

        for (int i = 0; i < branches.Length; ++i)
        {
            string branch = branches[i];
            bool isLast = i == branches.Length - 1;

            if (current == null)
            {
                current = new GameObject(branch).transform;
                continue;
            }

            Transform child = current.Find(branch);

            if (child != null && clean && isLast)
            {
                child.SetParent(null);
                Object.Destroy(child.gameObject);
                child = null;
            }

            if (child == null)
            {
                child = new GameObject(branch).transform;
                child.SetParent(current, false);
            }

            current = child;
        }

        return current;
    }
}