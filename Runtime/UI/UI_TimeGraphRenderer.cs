using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    [ExecuteAlways, RequireComponent(typeof(CanvasRenderer))]
    public sealed class UI_TimeGraphRenderer : Graphic
    {
        public readonly List<float> values = new();

        [Header("Fill")]
        public bool drawFill = true;
        public Color fillColor = Color.gray;

        [Header("Line")]
        public bool drawLine = true;
        public Color lineColor = Color.white;
        [Min(0.5f)] public float lineThickness = 2f;

        //--------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        [ContextMenu(nameof(TestContent))]
        void TestContent()
        {
            values.Clear();
            for (int i = 0; i < 25; i++)
                values.Add(Random.Range(0f, 1f));
            OnValidate();
        }

        [ContextMenu(nameof(OnValidate))]
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif

        //--------------------------------------------------------------------------------------------------------------

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (values.Count == 0)
                return;

            Rect r = rectTransform.rect;
            float width = r.width;
            float height = r.height;

            // --- convertit les valeurs en points locaux ---
            int count = values.Count;
            Vector2[] pts = new Vector2[count];
            for (int i = 0; i < count; ++i)
            {
                float t = (count == 1) ? 0f : i / (float)(count - 1);
                float x = r.xMin + t * width;
                float y = r.yMin + Mathf.Clamp01(values[i]) * height;
                pts[i] = new Vector2(x, y);
            }

            // =====================
            // 1) FILL SOUS LA COURBE
            // =====================
            if (drawFill)
            {
                int baseIndex = vh.currentVertCount;

                for (int i = 0; i < count; ++i)
                {
                    // haut (courbe)
                    vh.AddVert(pts[i], fillColor, Vector2.zero);
                    // bas (ligne de base)
                    vh.AddVert(new Vector2(pts[i].x, r.yMin), fillColor, Vector2.zero);
                }

                // chaque segment = 2 triangles formant un quad (haut/bas)
                for (int i = 0; i < count - 1; ++i)
                {
                    int i0 = baseIndex + 2 * i;
                    int i1 = baseIndex + 2 * i + 1;
                    int i2 = baseIndex + 2 * (i + 1);
                    int i3 = baseIndex + 2 * (i + 1) + 1;

                    vh.AddTriangle(i0, i1, i2);
                    vh.AddTriangle(i2, i1, i3);
                }
            }

            // =====================
            // 2) TRAIT PAR-DESSUS
            // =====================
            if (drawLine)
            {
                float half = lineThickness * 0.5f;
                int baseIndex = vh.currentVertCount;

                for (int i = 0; i < count - 1; ++i)
                {
                    Vector2 p1 = pts[i];
                    Vector2 p2 = pts[i + 1];

                    Vector2 dir = (p2 - p1).normalized;
                    if (dir.sqrMagnitude <= 0.000001f)
                        continue;

                    Vector2 normal = new Vector2(-dir.y, dir.x);
                    Vector2 off = normal * half;

                    // 4 sommets pour le segment [p1,p2]
                    Vector2 v0 = p1 + off;
                    Vector2 v1 = p1 - off;
                    Vector2 v2 = p2 + off;
                    Vector2 v3 = p2 - off;

                    int i0 = vh.currentVertCount;
                    vh.AddVert(v0, lineColor, Vector2.zero); // 0
                    vh.AddVert(v1, lineColor, Vector2.zero); // 1
                    vh.AddVert(v2, lineColor, Vector2.zero); // 2
                    vh.AddVert(v3, lineColor, Vector2.zero); // 3

                    // deux triangles pour le quad du trait
                    vh.AddTriangle(i0, i0 + 1, i0 + 2);
                    vh.AddTriangle(i0 + 2, i0 + 1, i0 + 3);
                }
            }
        }
    }
}