#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

static partial class Util_e_OLD
{
    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(UnparentTransform))]
    static void UnparentTransform(MenuCommand command) => ((Transform)command.context).SetParent(null, true);

    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(LogTypes))]
    static void LogTypes(MenuCommand command) => ((Transform)command.context).LogTypes();
    internal static void LogTypes(this Transform T)
    {
        foreach (var c in T.GetComponents<Component>())
            Debug.Log($"{c.GetType().FullName} ({c.GetType().Assembly})");
    }

    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(AddSceneSorter))]
    static void AddSceneSorter(MenuCommand command)
        => ((Transform)command.context).gameObject.AddComponent<SceneSorter>();

    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(NormalizeScales))]
    static void NormalizeScales(MenuCommand command)
    {
        foreach (Transform T in (Transform)command.context)
            T.localScale = Vector3.one;
    }

    [MenuItem("CONTEXT/" + nameof(RectTransform) + "/" + nameof(_EDITOR_) + "/" + nameof(GetPath))]
    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(GetPath))]
    static void GetPath(MenuCommand command)
    {
        string path = ((Transform)command.context).GetPath(false);
        Debug.Log(path);
        GUIUtility.systemCopyBuffer = path;
    }

    [MenuItem("CONTEXT/" + nameof(RectTransform) + "/" + nameof(_EDITOR_) + "/" + nameof(GetFullPath))]
    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(GetFullPath))]
    static void GetFullPath(MenuCommand command)
    {
        string path = ((Transform)command.context).GetPath(true);
        Debug.Log(path);
        GUIUtility.systemCopyBuffer = path;
    }

    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(GetSiblingIndex))]
    static void GetSiblingIndex(MenuCommand command) => Debug.Log(((Transform)command.context).GetSiblingIndex());

    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(NormalizeChildrenScales))]
    static void NormalizeChildrenScales(MenuCommand command) => ((Transform)command.context).NormalizeChildrenScales();

    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(LogChildCount))]
    static void LogChildCount(MenuCommand command) => Debug.Log(((Transform)command.context).childCount);

    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(LogChildCountRecursive))]
    static void LogChildCountRecursive(MenuCommand command) => Debug.Log(((Transform)command.context).GetComponentsInChildren<Transform>(true).Length - 1);
}
#endif