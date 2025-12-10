using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    [ExecuteAlways, RequireComponent(typeof(CanvasRenderer))]
    public class UI_TimeGraphRenderer : MaskableGraphic
    {
        [System.Serializable]
        public class TimeCurve
        {
            public readonly struct CurvePoint
            {
                public readonly float unscaledTime, value;
                public CurvePoint(in float value)
                {
                    unscaledTime = Time.unscaledTime;
                    this.value = value;
                }
            }

            public bool drawFillTop = true;
            public Color fillColorTop = new Color(0, 0, 0, .5f);

            public bool drawFillBottom = true;
            public Color fillColorBottom = new Color(0, 0, 0, .8f);

            public bool drawLine = true;
            public Color lineColor = new Color(0, 0, 0, 1);

            [Min(0.5f)] public float lineThickness = 1;

            public readonly List<CurvePoint> points = new();
        }

        [Range(0, 1)] public float time_offset;
        public bool drawGrid = true;
        [Min(1)] public int gridColumns = 8;
        [Min(1)] public int gridRows = 4;
        public float gridThickness = 1f;
        public Color gridColor = new Color(0, 0, 0, 0.15f);

        public List<TimeCurve> passes = new();

        readonly List<Vector2> draw_buffer = new();

        //--------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        [ContextMenu(nameof(TestContent))]
        public void TestContent()
        {
            TimeCurve pass = new();
            pass.points.AddRange(Enumerable.Range(0, 25).Select(_ => new TimeCurve.CurvePoint(Random.Range(0f, 1f))));
            passes.Add(pass);

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

            Rect r = rectTransform.rect;
            float width = r.width;
            float height = r.height;

            if (color.a > 0)
            {
                vh.AddVert(new(r.xMin, r.yMin), color, Vector2.zero);
                vh.AddVert(new(r.xMin, r.yMax), color, Vector2.zero);
                vh.AddVert(new(r.xMax, r.yMax), color, Vector2.zero);
                vh.AddVert(new(r.xMax, r.yMin), color, Vector2.zero);

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
            }

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

            if (passes != null)
                for (int i = 0; i < passes.Count; ++i)
                    if (passes[i].points != null)
                        if (passes[i].points.Count > 0)
                        {
                            TimeCurve pass = passes[i];

                            // --- convertit les valeurs en points locaux ---
                            int count = pass.points.Count;
                            Vector2[] draw_buffer = new Vector2[count];
                            for (int j = 0; j < count; ++j)
                            {
                                var point = pass.points[j];
                                float t = (count == 1) ? 0f : j / (float)(count - 1);
                                float x = r.xMin + t * width;
                                float y = r.yMin + Mathf.Clamp01(point.value) * height;
                                draw_buffer[j] = new Vector2(x, y);
                            }

                            // =====================
                            // 1) FILL AU-DESSUS LA COURBE
                            // =====================
                            if (pass.drawFillTop)
                            {
                                int baseIndex = vh.currentVertCount;

                                for (int j = 0; j < count; ++j)
                                {
                                    vh.AddVert(new Vector2(draw_buffer[j].x, r.yMax), pass.fillColorTop, Vector2.zero);
                                    vh.AddVert(draw_buffer[j], pass.fillColorTop, Vector2.zero);
                                }

                                // chaque segment = 2 triangles formant un quad (haut/bas)
                                for (int j = 0; j < count - 1; ++j)
                                {
                                    int i0 = baseIndex + 2 * j;
                                    int i1 = baseIndex + 2 * j + 1;
                                    int i2 = baseIndex + 2 * (j + 1);
                                    int i3 = baseIndex + 2 * (j + 1) + 1;

                                    vh.AddTriangle(i0, i1, i2);
                                    vh.AddTriangle(i2, i1, i3);
                                }
                            }

                            // =====================
                            // 1) FILL SOUS LA COURBE
                            // =====================
                            if (pass.drawFillBottom)
                            {
                                int baseIndex = vh.currentVertCount;

                                for (int j = 0; j < count; ++j)
                                {
                                    // haut (courbe)
                                    vh.AddVert(draw_buffer[j], pass.fillColorBottom, Vector2.zero);
                                    // bas (ligne de base)
                                    vh.AddVert(new Vector2(draw_buffer[j].x, r.yMin), pass.fillColorBottom, Vector2.zero);
                                }

                                // chaque segment = 2 triangles formant un quad (haut/bas)
                                for (int j = 0; j < count - 1; ++j)
                                {
                                    int i0 = baseIndex + 2 * j;
                                    int i1 = baseIndex + 2 * j + 1;
                                    int i2 = baseIndex + 2 * (j + 1);
                                    int i3 = baseIndex + 2 * (j + 1) + 1;

                                    vh.AddTriangle(i0, i1, i2);
                                    vh.AddTriangle(i2, i1, i3);
                                }
                            }

                            // =====================
                            // 3) TRAIT PAR-DESSUS
                            // =====================
                            if (pass.drawLine)
                            {
                                float half = pass.lineThickness * 0.5f;

                                for (int j = 0; j < count - 1; ++j)
                                {
                                    Vector2 p1 = draw_buffer[j];
                                    Vector2 p2 = draw_buffer[j + 1];

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
                                    vh.AddVert(v0, pass.lineColor, Vector2.zero); // 0
                                    vh.AddVert(v1, pass.lineColor, Vector2.zero); // 1
                                    vh.AddVert(v2, pass.lineColor, Vector2.zero); // 2
                                    vh.AddVert(v3, pass.lineColor, Vector2.zero); // 3

                                    // deux triangles pour le quad du trait
                                    vh.AddTriangle(i0, i0 + 1, i0 + 2);
                                    vh.AddTriangle(i0 + 2, i0 + 1, i0 + 3);
                                }
                            }
                        }
        }

        void AddQuadLine(in VertexHelper vh, in Vector2 p1, in Vector2 p2, in Color col, in float half)
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