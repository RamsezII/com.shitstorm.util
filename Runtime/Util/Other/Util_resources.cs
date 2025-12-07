using UnityEngine;

public static partial class Util
{
    public static T LoadResourceByType<T>() where T : Object => Resources.Load<T>(typeof(T).FullName);
    public static Object LoadResourceByType(in System.Type type) => Resources.Load(type.FullName, type);

    public static T InstantiateOrCreateIfAbsent<T>(in Transform parent = null, in FindObjectsInactive findObjectsInactive = FindObjectsInactive.Exclude) where T : MonoBehaviour
    {
        T clone = Object.FindAnyObjectByType<T>(findObjectsInactive);
        if (clone == null)
            return InstantiateOrCreate<T>(parent);
        return clone;
    }

    public static T Instantiate<T>(in Transform parent = null) where T : Component => (T)Instantiate(typeof(T), parent);
    public static Component Instantiate(in System.Type type, in Transform parent = null)
    {
        string name = type.FullName;
        Component resource = (Component)Resources.Load(name, type);

        Component clone = Object.Instantiate(resource, parent);
        clone.name = name;

        string log = $"instantiated \"{name}\"";
        if (parent != null)
            log += $" ({clone.transform.GetPath(true)})";

        Debug.Log(log.ToSubLog());
        return clone;
    }

    public static T InstantiateOrCreate<T>(in Transform parent = null) where T : Component => (T)InstantiateOrCreate(typeof(T), parent);
    public static Component InstantiateOrCreate(in System.Type type, in Transform parent = null)
    {
        if (type.IsAbstract)
            throw new System.ArgumentException($"Can not instantiate abstract type: \"{type}\"");

        string name = type.FullName;
        Component resource = (Component)Resources.Load(name, type);
        Component clone;
        string log;

        if (resource != null)
        {
            log = $"instantiated \"{name}\"";
            clone = Object.Instantiate(resource, parent);
            clone.name = name;
        }
        else
        {
            log = $"created \"{name}\"";
            if (parent == null)
                clone = new GameObject(name).AddComponent(type);
            else
                clone = parent.ForceFind(name).gameObject.AddComponent(type);
        }

        if (parent != null)
            log += $" ({clone.transform.GetPath(true)})";

        Debug.Log(log.ToSubLog());

        return clone;
    }
}