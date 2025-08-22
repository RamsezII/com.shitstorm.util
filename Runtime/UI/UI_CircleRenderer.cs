using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    public sealed class UI_CircleRenderer : UI_EmptyGraphic
    {
        public float tickness = 4;
        public int segments = 16;

        //--------------------------------------------------------------------------------------------------------------
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);

            Vector2 size = rectTransform.rect.size;
            float radius = .5f * Mathf.Min(size.x, size.y);

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            float angleStep = 360f / segments;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;

                vertex.position = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
                vh.AddVert(vertex);
                vertex.position = new Vector3(Mathf.Cos(angle) * (radius - tickness), Mathf.Sin(angle) * (radius - tickness));
                vh.AddVert(vertex);

                if (i > 0)
                {
                    vh.AddTriangle(2 * i, 2 * i + 1, 2 * i - 2);
                    vh.AddTriangle(2 * i + 1, 2 * i - 1, 2 * i - 2);
                }
            }
        }
    }
}