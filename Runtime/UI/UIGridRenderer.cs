using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    public sealed class UIGridRenderer : Graphic
    {
        public float tickness = 10f;

        //--------------------------------------------------------------------------------------------------------------

        [ContextMenu(nameof(SetVerticesDirty))]
        public override void SetVerticesDirty() => base.SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            //base.OnPopulateMesh(vh);

            vh.Clear();

            Rect r = rectTransform.rect;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            // out
            vertex.position = new(0, 0);
            vh.AddVert(vertex);
            vertex.position = new(r.width, 0);
            vh.AddVert(vertex);
            vertex.position = new(r.width, r.height);
            vh.AddVert(vertex);
            vertex.position = new(0, r.height);
            vh.AddVert(vertex);

            // in
            vertex.position = new(tickness, tickness);
            vh.AddVert(vertex);
            vertex.position = new(r.width - tickness, tickness);
            vh.AddVert(vertex);
            vertex.position = new(r.width - tickness, r.height - tickness);
            vh.AddVert(vertex);
            vertex.position = new(tickness, r.height - tickness);
            vh.AddVert(vertex);

            vh.AddTriangle(0, 4, 5);
            vh.AddTriangle(0, 5, 1);
            vh.AddTriangle(1, 5, 6);
            vh.AddTriangle(1, 6, 2);
            vh.AddTriangle(6, 2, 7);
            vh.AddTriangle(2, 7, 3);
            vh.AddTriangle(3, 7, 4);
            vh.AddTriangle(3, 4, 0);
        }
    }
}