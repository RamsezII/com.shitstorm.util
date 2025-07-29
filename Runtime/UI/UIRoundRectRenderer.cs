using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    public sealed class UIRoundRectRenderer : Graphic
    {
        public float tickness = .1f;

        //----------------------------------------------------------------------------------------------------------

        [ContextMenu(nameof(SetVerticesDirty))]
        public override void SetVerticesDirty() => base.SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            //base.OnPopulateMesh(vh);

            vh.Clear();

            Rect r = rectTransform.rect;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = new(0, 0);
            vh.AddVert(vertex);
            vertex.position = new(r.width, 0);
            vh.AddVert(vertex);
            vertex.position = new(r.width, r.height);
            vh.AddVert(vertex);
            vertex.position = new(0, r.height);
            vh.AddVert(vertex);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
        }
    }
}