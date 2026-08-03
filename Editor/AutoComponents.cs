#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _UTIL_.Editor
{
    internal static class AutoMeshColliderHierarchy
    {
        const string
            button_prefixe = nameof(GameObject) + "/" + nameof(AutoMeshColliderHierarchy) + "/",
            undo_name = nameof(AddMeshCollidersRecursively);

        //----------------------------------------------------------------------------------------------------------

        [MenuItem(itemName: button_prefixe + nameof(AddMeshCollidersRecursively), isValidateFunction: false, priority: 10)]
        private static void AddMeshCollidersRecursively(MenuCommand menuCommand)
        {
            GameObject[] roots = GetTargetRoots(menuCommand.context as GameObject);

            if (roots.Length == 0)
                return;

            int undoGroup = Undo.GetCurrentGroup();

            var visitedMeshFilters = new HashSet<MeshFilter>();

            int addedCount = 0;
            int updatedCount = 0;
            int missingMeshCount = 0;

            foreach (GameObject root in roots)
            {
                MeshFilter[] meshFilters =
                    root.GetComponentsInChildren<MeshFilter>(true);

                foreach (MeshFilter meshFilter in meshFilters)
                {
                    // Évite un double traitement si un parent et son enfant
                    // sont tous deux sélectionnés.
                    if (!visitedMeshFilters.Add(meshFilter))
                        continue;

                    Mesh mesh = meshFilter.sharedMesh;

                    if (mesh == null)
                    {
                        missingMeshCount++;
                        continue;
                    }

                    MeshCollider meshCollider =
                        meshFilter.GetComponent<MeshCollider>();

                    if (meshCollider == null)
                    {
                        meshCollider =
                            Undo.AddComponent<MeshCollider>(meshFilter.gameObject);

                        Undo.RecordObject(meshCollider, undo_name);

                        Rigidbody attachedRigidbody =
                            meshCollider.attachedRigidbody;

                        // Un Rigidbody dynamique exige un MeshCollider convexe.
                        meshCollider.convex =
                            attachedRigidbody != null &&
                            !attachedRigidbody.isKinematic;

                        meshCollider.isTrigger = false;
                        meshCollider.enabled = true;
                        meshCollider.sharedMesh = mesh;

                        RecordPrefabModification(meshCollider);

                        addedCount++;
                    }
                    else if (meshCollider.sharedMesh != mesh)
                    {
                        Undo.RecordObject(meshCollider, undo_name);

                        meshCollider.sharedMesh = mesh;

                        RecordPrefabModification(meshCollider);

                        updatedCount++;
                    }
                }
            }

            Undo.SetCurrentGroupName(undo_name);
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log(
                $"MeshColliders : {addedCount} ajouté(s), " +
                $"{updatedCount} réassigné(s), " +
                $"{missingMeshCount} MeshFilter(s) sans mesh.");
        }

        [MenuItem(button_prefixe, true)]
        private static bool CanAddMeshCollidersRecursively()
        {
            return Selection.gameObjects.Length > 0;
        }

        private static GameObject[] GetTargetRoots(GameObject context)
        {
            GameObject[] selection = Selection.gameObjects;

            if (context == null)
                return selection;

            // Si l'objet du clic droit appartient à une sélection multiple,
            // toute la sélection est traitée.
            foreach (GameObject selectedObject in selection)
            {
                if (selectedObject == context)
                    return selection;
            }

            return new[] { context };
        }

        private static void RecordPrefabModification(Object target)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(target))
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }
    }
}
#endif