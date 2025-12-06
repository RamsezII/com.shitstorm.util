#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;

partial class Util_e
{
    [MenuItem("Assets/Create/Animation/" + nameof(BlendTree))]
    static void CreateBlendTreeAsset()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrWhiteSpace(path))
            return;

        BlendTree bt = new();

        AssetDatabase.CreateAsset(bt, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(path);
        AssetDatabase.Refresh();

        Selection.activeObject = bt;
    }
}
#endif