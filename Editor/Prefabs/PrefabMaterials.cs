#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _UTIL_.Editor
{
    static class PrefabMterials
    {
        [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(_UTIL_) + "/" + nameof(ExtractMaterials))]
        static void ExtractMaterials(MenuCommand command) => ExtractMaterials(((Transform)command.context).gameObject);
        static void ExtractMaterials(GameObject root)
        {
            if (root == null)
                return;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

            // Un même Material peut être utilisé par plusieurs Renderers / slots.
            Dictionary<Material, Material> extracted = new();

            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (Renderer renderer in renderers)
                {
                    Material[] materials = renderer.sharedMaterials;
                    bool changed = false;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material source = materials[i];

                        if (source == null)
                            continue;

                        // Un Material embarqué dans un GLTF renvoie le chemin
                        // du GLTF qui le contient.
                        string sourcePath = AssetDatabase.GetAssetPath(source);

                        if (string.IsNullOrEmpty(sourcePath))
                            continue;

                        // Déjà un véritable .mat externe :
                        // on n'y touche pas.
                        if (Path.GetExtension(sourcePath)
                                .Equals(".mat", System.StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!extracted.TryGetValue(source, out Material clone))
                        {
                            string directory =
                                Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');

                            if (string.IsNullOrEmpty(directory))
                                continue;

                            clone = new Material(source)
                            {
                                name = source.name
                            };

                            string targetPath =
                                $"{directory}/{source.name}.mat";

                            targetPath =
                                AssetDatabase.GenerateUniqueAssetPath(targetPath);

                            AssetDatabase.CreateAsset(clone, targetPath);

                            extracted.Add(source, clone);
                        }

                        materials[i] = clone;
                        changed = true;
                    }

                    if (!changed)
                        continue;

                    Undo.RecordObject(renderer, "Extract Materials");

                    renderer.sharedMaterials = materials;

                    EditorUtility.SetDirty(renderer);

                    // Important si root est une instance de prefab.
                    if (PrefabUtility.IsPartOfPrefabInstance(renderer))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Extracted {extracted.Count} material(s) from {root.name}.",
                root
            );
        }
    }
}
#endif