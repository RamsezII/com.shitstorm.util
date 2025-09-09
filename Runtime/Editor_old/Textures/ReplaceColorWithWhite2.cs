#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

static class Util_e_textures
{
    public enum GrayMode
    {
        LumaLinear,
        LumaSRGB,
        ValueMax
    }

    //--------------------------------------------------------------------------------------------------------------

    [MenuItem("Assets/" + nameof(Util_e_textures) + "/" + nameof(ReplaceColorWithWhiteFunc_LumaLinear))]
    static void ReplaceColorWithWhiteFunc_LumaLinear() => ReplaceColorWithWhiteFunc(GrayMode.LumaLinear);

    [MenuItem("Assets/" + nameof(Util_e_textures) + "/" + nameof(ReplaceColorWithWhiteFunc_LumaSRGB))]
    static void ReplaceColorWithWhiteFunc_LumaSRGB() => ReplaceColorWithWhiteFunc(GrayMode.LumaSRGB);

    [MenuItem("Assets/" + nameof(Util_e_textures) + "/" + nameof(ReplaceColorWithWhiteFunc_ValueMax))]
    static void ReplaceColorWithWhiteFunc_ValueMax() => ReplaceColorWithWhiteFunc(GrayMode.ValueMax);

    static void ReplaceColorWithWhiteFunc(in GrayMode mode)
    {
        // Récupère les textures sélectionnées
        Object[] textures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        Texture2D originalTexture = (Texture2D)textures[0];

        string path = AssetDatabase.GetAssetPath(originalTexture);
        string directory = Path.GetDirectoryName(path);
        string filename = Path.GetFileNameWithoutExtension(path);

        Texture2D outTex = new(originalTexture.width, originalTexture.height, TextureFormat.ARGB32, false);
        Color[] px = originalTexture.GetPixels();

        for (int i = 0; i < px.Length; i++)
        {
            float a = px[i].a;
            float g;

            switch (mode)
            {
                // Rec.709 coefficients en LINEAR
                case GrayMode.LumaLinear:
                    {
                        // approx lin<->srgb (évite d’exiger le vrai import linéaire)
                        Vector3 srgb = new(px[i].r, px[i].g, px[i].b);
                        Vector3 lin = new(Mathf.Pow(srgb.x, 2.2f), Mathf.Pow(srgb.y, 2.2f), Mathf.Pow(srgb.z, 2.2f));
                        float l = 0.2126f * lin.x + 0.7152f * lin.y + 0.0722f * lin.z;
                        g = Mathf.Pow(Mathf.Clamp01(l), 1f / 2.2f); // retour “façon sRGB”
                        break;
                    }
                // Luma “gamma” (si tu préfères rester simple)
                case GrayMode.LumaSRGB:
                    g = 0.299f * px[i].r + 0.587f * px[i].g + 0.114f * px[i].b;
                    break;

                // Value = max canal (bon pour garder les highlights)
                case GrayMode.ValueMax:
                    g = Mathf.Max(px[i].r, Mathf.Max(px[i].g, px[i].b));
                    break;

                default: g = px[i].grayscale; break;
            }

            px[i] = new Color(g, g, g, a);
        }

        outTex.SetPixels(px);
        outTex.Apply(false, false);

        // Encode la nouvelle texture en PNG
        byte[] bytes = outTex.EncodeToPNG();

        // Sauvegarde la nouvelle texture
        string newPath = Path.Combine(directory, $"{filename}_{mode}.png");
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
}
#endif