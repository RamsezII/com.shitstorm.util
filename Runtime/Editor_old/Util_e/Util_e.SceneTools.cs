#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static partial class Util_e_OLD
{
    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(CheckMeshColliders))]
    static void CheckMeshColliders(MenuCommand command)
    {
        foreach (MeshCollider cld in ((Transform)command.context).GetComponentsInChildren<MeshCollider>(true))
            if (cld.sharedMesh == null)
            {
                Debug.Log(cld.name);
                if (cld.gameObject.TryGetComponent(out MeshFilter filter))
                    cld.sharedMesh = filter.sharedMesh;
                else
                    Debug.LogWarning("No mesh filter on " + cld.name);
            }
    }

    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(Unparent))]
    static void Unparent(MenuCommand command) => ((Transform)command.context).SetParent(null, true);

    [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_EDITOR_) + "/" + nameof(AllEmissiveMaterialsToBaked))]
    static void AllEmissiveMaterialsToBaked(MenuCommand command)
    {
        foreach (var renderer in ((Transform)command.context).GetComponentsInChildren<Renderer>(true))
            foreach (var mat in renderer.sharedMaterials)
                if (mat.HasProperty("_EmissionColor"))
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmission;
    }
}
#endif