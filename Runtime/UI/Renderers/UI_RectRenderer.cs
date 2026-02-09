using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    public sealed class UI_RectRenderer : Graphic
    {
        public Color
            top_left = Color.green,
            top_right = Color.red,
            bottom_left = Color.blue,
            bottom_right = Color.yellow;

        //--------------------------------------------------------------------------------------------------------------

        [ContextMenu(nameof(SetVerticesDirty))]
        public override void SetVerticesDirty() => base.SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);

            vh.Clear();

            Rect r = rectTransform.rect;
            UIVertex vertex = UIVertex.simpleVert;

            vertex.position = .5f * new Vector2(-r.width, r.height);
            vertex.color = color * top_left;
            vh.AddVert(vertex);

            vertex.position = .5f * new Vector2(r.width, r.height);
            vertex.color = color * top_right;
            vh.AddVert(vertex);

            vertex.position = .5f * new Vector2(r.width, -r.height);
            vertex.color = color * bottom_right;
            vh.AddVert(vertex);

            vertex.position = .5f * new Vector2(-r.width, -r.height);
            vertex.color = color * bottom_left;
            vh.AddVert(vertex);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}