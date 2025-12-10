using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    [ExecuteAlways, RequireComponent(typeof(CanvasRenderer))]
    public sealed class UI_GridRenderer : MaskableGraphic
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

            int count_h = (int)(r.width / spacing);
            int count_v = (int)(r.height / spacing);
            float ht = .5f * tickness;

            for (int i = 1; i <= count_h; i++)
            {
                vh.AddVert(new(i * spacing - ht, 0), color, Vector4.zero);
                vh.AddVert(new(i * spacing + ht, 0), color, Vector4.zero);
                vh.AddVert(new(i * spacing - ht, r.height), color, Vector4.zero);
                vh.AddVert(new(i * spacing + ht, r.height), color, Vector4.zero);

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
                vh.AddVert(new(0, j * spacing - ht), color, Vector4.zero);
                vh.AddVert(new(r.width, j * spacing - ht), color, Vector4.zero);
                vh.AddVert(new(0, j * spacing + ht), color, Vector4.zero);
                vh.AddVert(new(r.width, j * spacing + ht), color, Vector4.zero);

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