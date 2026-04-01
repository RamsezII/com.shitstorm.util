#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace _UTIL_e
{
    static class SelectedBlendTreeSaver
    {
        // Menu "Assets/..." (Project window)
        [MenuItem("Assets/Save Selected BlendTree as Asset", priority = 2010)]
        static void Save()
        {
            if (!TryGetSelectedBlendTree(out var src))
                return;

            var path = GetTargetPath("BlendTree.asset");
            if (string.IsNullOrEmpty(path))
                return;

            SaveCopy(src, path);
        }

        // Validation du menu
        [MenuItem("Assets/Save Selected BlendTree as Asset", validate = true)]
        static bool SaveSelectedBlendTreeAsAsset_Validate() => TryGetSelectedBlendTree(out _);

        // Menu contextuel Inspector sur un BlendTree (clic-droit header inspector)
        [MenuItem("CONTEXT/BlendTree/Save as Asset (.asset)")]
        static void SaveBlendTreeContext(MenuCommand command)
        {
            var src = command.context as BlendTree;
            if (!src) return;

            var path = GetTargetPath(src.name + ".asset");
            if (string.IsNullOrEmpty(path))
                return;

            SaveCopy(src, path);
        }

        static bool TryGetSelectedBlendTree(out BlendTree bt)
        {
            bt = null;

            // 1) Sélection directe
            if (Selection.activeObject is BlendTree direct)
            {
                bt = direct;
                return true;
            }

            // 2) Sélection d'un AnimatorState dans l'Animator (parfois ça marche selon focus)
            if (Selection.activeObject is AnimatorState state && state.motion is BlendTree fromState)
            {
                bt = fromState;
                return true;
            }

            // 3) Sélection d'un AnimatorState via AnimatorControllerLayer/StateMachine etc (rare)
            // Rien d'autre de fiable sans API interne.
            return false;
        }

        static string GetTargetPath(string defaultFileName)
        {
            // Base = dossier de l'asset sélectionné dans le Project
            var basePath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrWhiteSpace(basePath))
                basePath = "Assets";

            // Si c'est un fichier, prendre son dossier
            if (File.Exists(basePath))
                basePath = Path.GetDirectoryName(basePath);

            if (string.IsNullOrEmpty(basePath))
                basePath = "Assets";

            var path = Path.Combine(basePath, defaultFileName).Replace("\\", "/");
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            return path;
        }

        static void SaveCopy(BlendTree src, string assetPath)
        {
            // Crée une nouvelle instance et copie tout le contenu sérialisé
            var copy = new BlendTree
            {
                name = string.IsNullOrEmpty(src.name) ? "BlendTree" : src.name
            };

            EditorUtility.CopySerialized(src, copy);
            copy.hideFlags = HideFlags.None; // important si src était sub-asset avec flags bizarres

            AssetDatabase.CreateAsset(copy, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();

            Selection.activeObject = copy;
            EditorGUIUtility.PingObject(copy);
        }
    }
}
#endif