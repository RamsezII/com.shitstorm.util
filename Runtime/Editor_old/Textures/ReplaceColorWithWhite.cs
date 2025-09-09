#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

partial class Util_e_OLD
{
    [MenuItem("Assets/" + nameof(Util_e_OLD) + "/" + nameof(ReplaceColorWithWhite))]
    static void ReplaceColorWithWhite()
    {
        // Récupère les textures sélectionnées
        Object[] textures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);

        foreach (Object obj in textures)
        {
            Texture2D originalTexture = obj as Texture2D;

            if (originalTexture == null)
                continue;

            string path = AssetDatabase.GetAssetPath(originalTexture);
            string directory = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);

            // Charge l'importeur de texture pour modifier les paramètres
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (textureImporter == null)
                continue;

            // Assure que la texture est lisible
            if (!textureImporter.isReadable)
            {
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();
            }

            Texture2D newTexture = ReplaceColorWithWhiteFunc(originalTexture);

            // Encode la nouvelle texture en PNG
            byte[] bytes = newTexture.EncodeToPNG();

            // Sauvegarde la nouvelle texture
            string newPath = Path.Combine(directory, filename + "_white.png");
            File.WriteAllBytes(newPath, bytes);

            // Importe la nouvelle texture dans le projet
            AssetDatabase.ImportAsset(newPath);

            // Configure l'importeur de la nouvelle texture si nécessaire
            TextureImporter newTextureImporter = AssetImporter.GetAtPath(newPath) as TextureImporter;
            if (newTextureImporter != null)
            {
                newTextureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
                newTextureImporter.isReadable = true;
                newTextureImporter.SaveAndReimport();
            }
        }

        AssetDatabase.Refresh();

        Debug.Log("Textures modifiées avec succès !");
    }

    static Texture2D ReplaceColorWithWhiteFunc(Texture2D texture)
    {
        // Crée une nouvelle texture avec les mêmes dimensions
        Texture2D newTexture = new(texture.width, texture.height, TextureFormat.ARGB32, false);

        // Récupère tous les pixels de la texture originale
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            // Conserve l'alpha et remplace la couleur par du blanc
            float alpha = pixels[i].a;
            pixels[i] = new Color(1f, 1f, 1f, alpha);
        }

        // Applique les changements à la nouvelle texture
        newTexture.SetPixels(pixels);
        newTexture.Apply();

        return newTexture;
    }
}
#endif