#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

partial class Util_e_OLD
{
    [MenuItem("Assets/" + nameof(_EDITOR_) + "/" + nameof(UnloadUnusedAssetsInResources))]
    static void UnloadUnusedAssetsInResources()
    {
        Resources.UnloadUnusedAssets();
        Debug.Log("🧼 Resources unloaded");
    }

    [MenuItem("Assets/" + nameof(_EDITOR_) + "/" + nameof(ForceGarbageCollection))]
    static void ForceGarbageCollection()
    {
        GC.Collect();
        Debug.Log("🧼 GarbageCollection");
    }
}
#endif