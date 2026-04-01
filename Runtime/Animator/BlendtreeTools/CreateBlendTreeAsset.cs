#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;

namespace _UTIL_e
{
    static class BlendTreeAssetCreator
    {
        [MenuItem("Assets/Create/Animation/Animation BlendTree")]
        static void Create()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!AssetDatabase.IsValidFolder(path))
                path = Path.GetDirectoryName(path);

            path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, "BlendTree.asset"));

            BlendTree bt = new();

            AssetDatabase.CreateAsset(bt, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

            Selection.activeObject = bt;
        }
    }
}
#endif