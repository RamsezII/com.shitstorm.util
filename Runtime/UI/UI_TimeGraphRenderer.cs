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
            [System.Serializable]
            public struct CurvePoint
            {
                public float time, value;
                public CurvePoint(in float value)
                {
                    time = Time.unscaledTime;
                    this.value = value;
                }
#if UNITY_EDITOR
                public override readonly string ToString() => $"{{ {nameof(time)}: {System.Math.Round(time, 1)} ; {nameof(value)}: {System.Math.Round(value, 1)} }}";
                public CurvePoint(in float time, in float value)
                {
                    this.time = time;
                    this.value = value;
                }
#endif
            }

            [System.Serializable]
            public struct Options
            {
                public float min, max;

                public bool drawFillTop;
                public Color fillColorTop;

                public bool drawFillBottom;
                public Color fillColorBottom;

                public bool drawLine;
                public Color lineColor;

                [Min(0.5f)] public float lineThickness;
            }

            public float accumulatedValue;

            public Options options = new()
            {
                min = 1,
                max = 1,

                drawFillTop = true,
                fillColorTop = new Color(0, 0, 0, .5f),

                drawFillBottom = true,
                fillColorBottom = new Color(0, 0, 0, .8f),

                drawLine = true,
                lineColor = new Color(0, 0, 0, 1),

                lineThickness = 1,
            };

            public List<CurvePoint> points = new();

            //--------------------------------------------------------------------------------------------------------------

            internal TimeCurve()
            {
            }

            //--------------------------------------------------------------------------------------------------------------

            public void Accumulate(in float value)
            {
                lock (this)
                    accumulatedValue += value;
            }

            public void AddPoint(in float value = 0)
            {
                lock (this)
                {
                    accumulatedValue += value;
                    points.Add(new CurvePoint(accumulatedValue));
                    accumulatedValue = 0;
                }
            }
        }

        public float timeLate = 1, timeSpan = 5;

        public bool drawGrid = true;
        [Min(1)] public int gridColumns = 8;
        [Min(1)] public int gridRows = 4;
        public float gridThickness = 1f;
        public Color gridColor = new Color(0, 0, 0, 0.15f);

#if UNITY_EDITOR
        [SerializeField] int _drawFrame;
        [SerializeField] float _testTime_current;
#endif

        public List<TimeCurve> curves = new();

        readonly List<Vector2> positions_buffer = new();

        //--------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        [ContextMenu(nameof(AddRandomPass))]
        public TimeCurve AddRandomPass()
        {
            TimeCurve pass = new();
            pass.points.AddRange(Enumerable.Range(0, 10).Select(i => new TimeCurve.CurvePoint(i, Random.Range(0f, 1f))));
            curves.Add(pass);
            OnValidate();
            return pass;
        }

        [ContextMenu(nameof(OnValidate))]
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif

        //--------------------------------------------------------------------------------------------------------------

        public TimeCurve AddCurve()
        {
            TimeCurve pass = new();
            curves.Add(pass);
            return pass;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float time = Time.unscaledTime;

#if UNITY_EDITOR
            ++_drawFrame;
            if (!Application.isPlaying)
                time = _testTime_current;
#endif

            Rect r = rectTransform.rect;

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
                    float x = r.xMin + r.width * t;

                    Vector2 p1 = new(x, r.yMin);
                    Vector2 p2 = new(x, r.yMax);

                    AddQuadLine(vh, p1, p2, gridColor, half);
                }

                // Horizontal lines
                for (int rIdx = 0; rIdx <= gridRows; ++rIdx)
                {
                    float t = rIdx / (float)gridRows;
                    float y = r.yMin + r.height * t;

                    Vector2 p1 = new(r.xMin, y);
                    Vector2 p2 = new(r.xMax, y);

                    AddQuadLine(vh, p1, p2, gridColor, half);
                }
            }

            if (curves != null)
            {
                float time_b = time - timeLate;
                float time_a = time_b - timeSpan;

                for (int i = 0; i < curves.Count; ++i)
                    if (curves[i].points != null)
                        if (curves[i].points.Count > 0)
                        {
                            positions_buffer.Clear();

                            TimeCurve curve = curves[i];

                            // --- convertit les valeurs en points locaux ---
                            for (int j = 0; j < curve.points.Count - 1; ++j)
                            {
                                var point1 = curve.points[j];
                                var point2 = curve.points[j + 1];

                                if (point2.time <= time_a)
                                    curve.points.RemoveAt(j--);

                                float lerp = Util.InverseLerpUnclamped(time_a, time_b, point1.time);
                                float x = r.xMin + r.width * lerp;
                                float y = r.yMin + r.height * point1.value;

                                positions_buffer.Add(new Vector2(x, y));
                            }

                            if (positions_buffer.Count == 0)
                                continue;

                            // =====================
                            // 1) FILL AU-DESSUS LA COURBE
                            // =====================
                            if (curve.options.drawFillTop)
                            {
                                int baseIndex = vh.currentVertCount;

                                for (int j = 0; j < positions_buffer.Count; ++j)
                                {
                                    vh.AddVert(new Vector2(positions_buffer[j].x, r.yMax), curve.options.fillColorTop, Vector2.zero);
                                    vh.AddVert(positions_buffer[j], curve.options.fillColorTop, Vector2.zero);
                                }

                                // chaque segment = 2 triangles formant un quad (haut/bas)
                                for (int j = 0; j < positions_buffer.Count - 1; ++j)
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
                            if (curve.options.drawFillBottom)
                            {
                                int baseIndex = vh.currentVertCount;

                                for (int j = 0; j < positions_buffer.Count; ++j)
                                {
                                    // haut (courbe)
                                    vh.AddVert(positions_buffer[j], curve.options.fillColorBottom, Vector2.zero);
                                    // bas (ligne de base)
                                    vh.AddVert(new Vector2(positions_buffer[j].x, r.yMin), curve.options.fillColorBottom, Vector2.zero);
                                }

                                // chaque segment = 2 triangles formant un quad (haut/bas)
                                for (int j = 0; j < positions_buffer.Count - 1; ++j)
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
                            if (curve.options.drawLine)
                            {
                                float half = curve.options.lineThickness * 0.5f;

                                for (int j = 0; j < positions_buffer.Count - 1; ++j)
                                {
                                    Vector2 p1 = positions_buffer[j];
                                    Vector2 p2 = positions_buffer[j + 1];

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
                                    vh.AddVert(v0, curve.options.lineColor, Vector2.zero); // 0
                                    vh.AddVert(v1, curve.options.lineColor, Vector2.zero); // 1
                                    vh.AddVert(v2, curve.options.lineColor, Vector2.zero); // 2
                                    vh.AddVert(v3, curve.options.lineColor, Vector2.zero); // 3

                                    // deux triangles pour le quad du trait
                                    vh.AddTriangle(i0, i0 + 1, i0 + 2);
                                    vh.AddTriangle(i0 + 2, i0 + 1, i0 + 3);
                                }
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