using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    [ExecuteAlways, RequireComponent(typeof(CanvasRenderer))]
    public sealed class UI_NormalizedGridRenderer : MaskableGraphic
    {
        public float thickness = 10;
        public int count_h = 5, count_v = 5;

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
            float w = r.width;
            float h = r.height;
            float th = .5f * thickness;

            for (int i = 1; i <= count_h; i++)
            {
                float lerp = (float)i / count_h;
                vh.AddVert(new Vector2(i * w - th, 0), color, Vector4.zero);
                vh.AddVert(new Vector2(i * w - th, h), color, Vector4.zero);
                vh.AddVert(new Vector2(i * w + th, h), color, Vector4.zero);
                vh.AddVert(new Vector2(i * w + th, 0), color, Vector4.zero);

                vh.AddTriangle(
                    vh.currentVertCount - 4,
                    vh.currentVertCount - 3,
                    vh.currentVertCount - 2
                    );

                vh.AddTriangle(
                    vh.currentVertCount - 2,
                    vh.currentVertCount - 1,
                    vh.currentVertCount - 4
                    );
            }
        }
    }
}