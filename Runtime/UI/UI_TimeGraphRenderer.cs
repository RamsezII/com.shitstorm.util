using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    [ExecuteAlways, RequireComponent(typeof(CanvasRenderer))]
    public class UI_TimeGraphRenderer : MaskableGraphic
    {
        public readonly List<float> values = new();

        public bool drawGrid = true;
        [Min(1)] public int gridColumns = 8;
        [Min(1)] public int gridRows = 4;
        public float gridThickness = 1f;
        public Color gridColor = new Color(0, 0, 0, 0.15f);

        public bool drawFillTop = true;
        public Color fillColorTop = new Color(0, 0, 0, .5f);

        public bool drawFillBottom = true;
        public Color fillColorBottom = new Color(0, 0, 0, .8f);

        public bool drawLine = true;
        public Color lineColor = new Color(0, 0, 0, 1);

        [Min(0.5f)] public float lineThickness = 2f;

        //--------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        [ContextMenu(nameof(TestContent))]
        public void TestContent()
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

            // ---------------- GRID ----------------
            if (drawGrid)
            {
                float half = gridThickness * 0.5f;

                // Vertical lines
                for (int c = 0; c <= gridColumns; ++c)
                {
                    float t = c / (float)gridColumns;
                    float x = r.xMin + t * width;

                    Vector2 p1 = new Vector2(x, r.yMin);
                    Vector2 p2 = new Vector2(x, r.yMax);

                    AddQuadLine(vh, p1, p2, gridColor, half);
                }

                // Horizontal lines
                for (int rIdx = 0; rIdx <= gridRows; ++rIdx)
                {
                    float t = rIdx / (float)gridRows;
                    float y = r.yMin + t * height;

                    Vector2 p1 = new Vector2(r.xMin, y);
                    Vector2 p2 = new Vector2(r.xMax, y);

                    AddQuadLine(vh, p1, p2, gridColor, half);
                }
            }

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
            // 1) FILL AU-DESSUS LA COURBE
            // =====================
            if (drawFillTop)
            {
                int baseIndex = vh.currentVertCount;

                for (int i = 0; i < count; ++i)
                {
                    vh.AddVert(new Vector2(pts[i].x, r.yMax), fillColorTop, Vector2.zero);
                    vh.AddVert(pts[i], fillColorTop, Vector2.zero);
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
            // 1) FILL SOUS LA COURBE
            // =====================
            if (drawFillBottom)
            {
                int baseIndex = vh.currentVertCount;

                for (int i = 0; i < count; ++i)
                {
                    // haut (courbe)
                    vh.AddVert(pts[i], fillColorBottom, Vector2.zero);
                    // bas (ligne de base)
                    vh.AddVert(new Vector2(pts[i].x, r.yMin), fillColorBottom, Vector2.zero);
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
            // 3) TRAIT PAR-DESSUS
            // =====================
            if (drawLine)
            {
                float half = lineThickness * 0.5f;

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

        void AddQuadLine(VertexHelper vh, Vector2 p1, Vector2 p2, Color col, float half)
        {
            Vector2 dir = p2 - p1;
            if (dir.sqrMagnitude < 0.000001f)
                return;

            dir.Normalize();
            Vector2 normal = new Vector2(-dir.y, dir.x);
            Vector2 off = normal * half;

            Vector2 v0 = p1 + off;
            Vector2 v1 = p1 - off;
            Vector2 v2 = p2 + off;
            Vector2 v3 = p2 - off;

            int i0 = vh.currentVertCount;
            vh.AddVert(v0, col, Vector2.zero);
            vh.AddVert(v1, col, Vector2.zero);
            vh.AddVert(v2, col, Vector2.zero);
            vh.AddVert(v3, col, Vector2.zero);

            vh.AddTriangle(i0, i0 + 1, i0 + 2);
            vh.AddTriangle(i0 + 2, i0 + 1, i0 + 3);
        }
    }
}