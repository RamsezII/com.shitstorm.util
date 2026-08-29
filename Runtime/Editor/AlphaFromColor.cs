#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace _UTIL_e
{
    static class TextureAlphaTools
    {
        [MenuItem("Assets/" + nameof(_UTIL_) + "/" + nameof(ReplaceBlackWithAlpha))]
        static void ReplaceBlackWithAlpha()
        {
            Texture2D tex = Selection.activeObject as Texture2D;
            if (tex == null)
            {
                Debug.LogError("Aucune texture sélectionnée !");
                return;
            }

            string path = AssetDatabase.GetAssetPath(tex);
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);
            string defaultName = tex.name + "_alpha.png";

            // Ouvre une fenêtre pour choisir le fichier de sortie
            string savePath = EditorUtility.SaveFilePanel(
                "Enregistrer la texture transformée",
                directory,
                defaultName,
                "png"
            );

            if (string.IsNullOrEmpty(savePath))
                return; // utilisateur a annulé

            // Rendre la texture lisible si nécessaire
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            // Copie de la texture
            Texture2D src = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            Graphics.CopyTexture(tex, src);

            Color[] pixels = src.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                float luminance = (c.r + c.g + c.b) / 3f;
                c.a = Mathf.SmoothStep(0, 1, luminance); // gradient doux
                pixels[i] = new Color(c.r, c.g, c.b, c.a);
            }

            src.SetPixels(pixels);
            src.Apply();

            byte[] png = src.EncodeToPNG();
            File.WriteAllBytes(savePath, png);

            AssetDatabase.Refresh();
            Debug.Log($"✅ Texture exportée avec alpha basé sur la luminosité : {savePath}");
        }
    }
}
#endif