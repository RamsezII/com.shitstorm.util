using UnityEditor;
using System.IO;
using UnityEngine;

namespace _UTIL_e
{
    static internal class PackageJsonCreator
    {
        const string
            button_name = "Assets/" + nameof(_UTIL_e) + "/" + nameof(CreatePackageJson);

        //--------------------------------------------------------------------------------------------------------------

        [MenuItem(button_name)]
        static void CreatePackageJson()
        {
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (!AssetDatabase.IsValidFolder(selectedPath))
                selectedPath = Path.GetDirectoryName(selectedPath);

            string packageJsonPath = Path.Combine(selectedPath, "package.json");

            if (File.Exists(packageJsonPath))
                if (!EditorUtility.DisplayDialog(
                    "package.json existe déjà",
                    "Un package.json existe déjà dans ce dossier. Voulez-vous le remplacer ?",
                    "Oui",
                    "Non"
                ))
                    return;

            string template =
@"{
    ""name"": ""com.ram16.<name>"",
    ""version"": ""7.7.7"",
    ""displayName"": ""ram16.<namespace>"",
    ""description"": ""Fragile and immense, seed and tree — seed as central Heart."",
    ""author"": {
        ""name"": ""<author>"",
        ""url"": ""https://github.com/<author>""
    }
}";

            File.WriteAllText(packageJsonPath, template);
            AssetDatabase.Refresh();

            Debug.Log(packageJsonPath + " créé");
        }
    }
}