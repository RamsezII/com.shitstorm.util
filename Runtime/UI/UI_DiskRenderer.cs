using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    public sealed class UI_DiskRenderer : Graphic
    {
        public float radius = 50f;
        public int segments = 16;

        //--------------------------------------------------------------------------------------------------------------
        [ContextMenu(nameof(SetVerticesDirty))]
        public override void SetVerticesDirty() => base.SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            float angleStep = 360f / segments;

            vh.AddVert(vertex); // Center vertex

            for (int i = 0; i <= segments; i++)
                if (i == segments)
                    vh.AddTriangle(0, 1, i); // Connect last segment to the first
                else
                {
                    float angle = i * angleStep * Mathf.Deg2Rad;
                    vertex.position = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

                    vh.AddVert(vertex);

                    vh.AddTriangle(0, i + 1, i);
                }
        }
    }
}