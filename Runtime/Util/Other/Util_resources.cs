using UnityEngine;

public static partial class Util
{
    public static T LoadResourceByType<T>() where T : Object => Resources.Load<T>(typeof(T).FullName);
    public static Object LoadResourceByType(in System.Type type) => Resources.Load(type.FullName, type);

    public static T InstantiateOrCreateIfAbsent<T>(in Vector3 position = default, in Quaternion rotation = default, in Transform parent = null, in FindObjectsInactive findObjectsInactive = FindObjectsInactive.Exclude) where T : MonoBehaviour
    {
        T clone = Object.FindAnyObjectByType<T>(findObjectsInactive);
        if (clone == null)
            return InstantiateOrCreate<T>(position, rotation, parent);
        return clone;
    }

    public static T InstantiateOrCreate<T>(in Vector3 position = default, in Quaternion rotation = default, in Transform parent = null) where T : Component => InstantiateOrCreate<T>(typeof(T), position, rotation, parent);
    public static Component InstantiateOrCreate(in System.Type type, in Vector3 position = default, in Quaternion rotation = default, in Transform parent = null) => InstantiateOrCreate<Component>(type, position, rotation, parent);
    public static T InstantiateOrCreate<T>(in System.Type type, in Vector3 position = default, Quaternion rotation = default, in Transform parent = null) where T : Component
    {
        if (type.IsAbstract)
            throw new System.ArgumentException($"Can not instantiate abstract type: \"{type}\"");

        rotation = rotation == default ? Quaternion.identity : rotation;

        string name = type.FullName;
        T resource = (T)Resources.Load(name, type);
        T clone;
        string log;

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
                clone = (T)parent.ForceFind(name).gameObject.AddComponent(type);
            clone.transform.SetLocalPositionAndRotation(position, rotation);
        }

        if (parent != null)
            log += $" ({clone.transform.GetPath(true)})";

        Debug.Log(log.ToSubLog());

        return clone;
    }
}