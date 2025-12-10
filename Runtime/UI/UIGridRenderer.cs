using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    [ExecuteAlways, RequireComponent(typeof(CanvasRenderer))]
    public sealed class UIGridRenderer : Graphic
    {
        public float
            tickness = 10f,
            spacing = 100f;

        //--------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
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
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            int count_h = (int)(r.width / spacing);
            int count_v = (int)(r.height / spacing);
            float ht = .5f * tickness;

            for (int i = 1; i <= count_h; i++)
            {
                vertex.position = new(i * spacing - ht, 0);
                vh.AddVert(vertex);
                vertex.position = new(i * spacing + ht, 0);
                vh.AddVert(vertex);
                vertex.position = new(i * spacing - ht, r.height);
                vh.AddVert(vertex);
                vertex.position = new(i * spacing + ht, r.height);
                vh.AddVert(vertex);

                vh.AddTriangle(
                    vh.currentVertCount - 4,
                    vh.currentVertCount - 3,
                    vh.currentVertCount - 2
                    );

                vh.AddTriangle(
                    vh.currentVertCount - 3,
                    vh.currentVertCount - 1,
                    vh.currentVertCount - 2
                    );
            }

            for (int j = 1; j <= count_v; j++)
            {
                vertex.position = new(0, j * spacing - ht);
                vh.AddVert(vertex);
                vertex.position = new(r.width, j * spacing - ht);
                vh.AddVert(vertex);
                vertex.position = new(0, j * spacing + ht);
                vh.AddVert(vertex);
                vertex.position = new(r.width, j * spacing + ht);
                vh.AddVert(vertex);

                vh.AddTriangle(
                    vh.currentVertCount - 4,
                    vh.currentVertCount - 3,
                    vh.currentVertCount - 2
                    );

                vh.AddTriangle(
                    vh.currentVertCount - 3,
                    vh.currentVertCount - 1,
                    vh.currentVertCount - 2
                    );
            }
        }
    }
}