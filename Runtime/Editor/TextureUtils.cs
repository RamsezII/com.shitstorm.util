#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace _UTIL_e
{
    static class TextureUtils
    {
        [MenuItem("Assets/" + nameof(_UTIL_) + "/" + nameof(TextureUtils) + "/" + nameof(CreateCircularGlow64))]
        public static void CreateCircularGlow64()
        {
            const int W = 64, H = 64;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false, true); // linear

            float cx = (W - 1) * 0.5f;
            float cy = (H - 1) * 0.5f;
            float radius = Mathf.Min(W, H) * 0.5f - 0.5f; // jusqu’au bord

            // Pixels
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    float dx = (x - cx) / radius;
                    float dy = (y - cy) / radius;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);   // 0 au centre, ~1 au bord
                    float a = 1f - Mathf.SmoothStep(0f, 1f, r); // falloff doux centre→bord
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply(false);

            // Choix du chemin de sortie (même dossier que l’asset sélectionné si possible)
            string defaultDir = Application.dataPath;
            var sel = Selection.activeObject;
            if (sel != null)
            {
                string apath = AssetDatabase.GetAssetPath(sel);
                if (!string.IsNullOrEmpty(apath))
                    defaultDir = Path.GetDirectoryName(Path.GetFullPath(apath));
            }

            string savePath = EditorUtility.SaveFilePanel(
                "Enregistrer le glow 64x64",
                defaultDir,
                "glow_64.png",
                "png"
            );
            if (string.IsNullOrEmpty(savePath)) return;

            File.WriteAllBytes(savePath, tex.EncodeToPNG());
            AssetDatabase.Refresh();
            Debug.Log("✅ Glow 64x64 enregistré : " + savePath);
        }
    }
}
#endif