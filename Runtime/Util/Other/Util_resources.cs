using UnityEngine;

public static partial class Util
{
    public static T LoadResourceByType<T>() where T : Object => Resources.Load<T>(typeof(T).FullName);
    public static Object LoadResourceByType(in System.Type type) => Resources.Load(type.FullName, type);

    public static T InstantiateOrCreateIfAbsent<T>(in Transform parent = null, in FindObjectsInactive findObjectsInactive = FindObjectsInactive.Exclude) where T : MonoBehaviour
    {
        T clone = Object.FindAnyObjectByType<T>(findObjectsInactive);
        if (clone == null)
            return InstantiateOrCreate<T>(parent: parent);
        return clone;
    }

    public static Component InstantiateOrCreateIfAbsent(in System.Type type, in Transform parent = null, in FindObjectsInactive findObjectsInactive = FindObjectsInactive.Exclude)
    {
        GameObject clone = (GameObject)Object.FindAnyObjectByType(type, findObjectsInactive);
        if (clone == null)
            return InstantiateOrCreate(type: type, parent: parent);
        return clone.GetComponent(type);
    }

    public static T InstantiateOrCreate<T>(in Vector3 position = default, in Quaternion rotation = default, in Transform parent = null) where T : Component => InstantiateOrCreate<T>(typeof(T), position, rotation, parent);
    public static Component InstantiateOrCreate(in System.Type type, in Vector3 position = default, in Quaternion rotation = default, in Transform parent = null) => InstantiateOrCreate<Component>(type, position, rotation, parent);
    public static T InstantiateOrCreate<T>(in System.Type type, in Vector3 position = default, Quaternion rotation = default, in Transform parent = null) where T : Component
    {
        if (type.IsAbstract)
            throw new System.ArgumentException($"Can not instantiate abstract type: \"{type}\"");

        string name = type.FullName;
        T resource = (T)Resources.Load(name, type);
        T clone;
        string log;

        rotation = rotation == default ? Quaternion.identity : rotation;

        if (resource != null)
        {
            log = $"instantiated \"{name}\"";
            clone = Object.Instantiate(resource, position, rotation, parent);
            clone.name = name;
        }
        else
        {
            log = $"created \"{name}\"";
            if (parent == null)
                clone = (T)new GameObject(name).AddComponent(type);
            else
            {
                Transform tfm = parent.Find(name);
                if (tfm == null)
                    tfm = parent.ForceFind(name, true);
                else
                {
                    Transform ptfm = tfm.parent;
                    tfm = new GameObject(name).transform;
                    tfm.SetParent(ptfm, false);
                }
                clone = (T)tfm.gameObject.AddComponent(type);
            }

            clone.transform.SetLocalPositionAndRotation(position, rotation);
        }

        if (parent != null)
            log += $" ({clone.transform.GetPath(true)})";

        if (clone.transform is RectTransform rt)
            rt.FillParent();

        Debug.Log(log.ToSubLog());

        return clone;
    }

    public static GameObject InstantiateOrCreate(in string resource_name, in Vector3 position = default, Quaternion rotation = default, in Transform parent = null)
    {
        var resource = Resources.Load<GameObject>(resource_name);
        GameObject clone;
        string log;

        rotation = rotation == default ? Quaternion.identity : rotation;

        if (resource != null)
        {
            log = $"instantiated \"{resource_name}\"";
            clone = Object.Instantiate(resource, position, rotation, parent);
            clone.name = resource_name;
        }
        else
        {
            log = $"created \"{resource_name}\"";
            if (parent == null)
                clone = new GameObject(resource_name);
            else
            {
                Transform tfm = parent.Find(resource_name);
                if (tfm == null)
                    tfm = parent.ForceFind(resource_name, true);
                else
                {
                    Transform ptfm = tfm.parent;
                    tfm = new GameObject(resource_name).transform;
                    tfm.SetParent(ptfm, false);
                }
                clone = tfm.gameObject;
            }

            clone.transform.SetLocalPositionAndRotation(position, rotation);
        }

        if (parent != null)
            log += $" ({clone.transform.GetPath(true)})";

        Debug.Log(log.ToSubLog());

        return clone;
    }

    public static bool TryInstantiateByName(in string resource_name, out Object clone, in Vector3 position = default, Quaternion rotation = default, in Transform parent = null)
    {
        var resource = Resources.Load(resource_name);

        if (resource != null)
        {
            string log = $"instantiated \"{resource_name}\"";
            rotation = rotation == default ? Quaternion.identity : rotation;
            clone = Object.Instantiate(resource, position, rotation, parent);
            clone.name = resource_name;

            if (parent != null && clone is GameObject go)
                log += $" ({go.transform.GetPath(true)})";

            Debug.Log(log.ToSubLog());
            return true;
        }

        clone = null;
        return false;
    }
}