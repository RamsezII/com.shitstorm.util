using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    public sealed class UI_GradientRenderer : Graphic
    {
        [Serializable]
        public struct Pair
        {
            public float position;
            public Color color;
            public Pair(in float position, in Color color)
            {
                this.position = position;
                this.color = color;
            }
        }

        public List<Pair> values = new()
        {
            new Pair(0f, Color.red),
            new Pair(.5f, Color.green),
            new Pair(1f, Color.blue),
        };

        //--------------------------------------------------------------------------------------------------------------

        [ContextMenu(nameof(SetVerticesDirty))]
        public override void SetVerticesDirty() => base.SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);

            vh.Clear();

            if (values.Count < 2)
                return;

            Rect r = rectTransform.rect;
            UIVertex vertex = UIVertex.simpleVert;

            var first = values[0];
            var last = values[^1];

            vertex.position = .5f * new Vector2(-r.width, -r.height);
            vertex.color = color * first.color;
            vh.AddVert(vertex);

            vertex.position = .5f * new Vector2(-r.width, r.height);
            vertex.color = color * first.color;
            vh.AddVert(vertex);

            for (int i = 1; i < values.Count; i++)
            {
                var pair = values[i];
                float x = r.width * (-.5f + Mathf.InverseLerp(first.position, last.position, pair.position));

                vertex.position = new Vector2(x, -.5f * r.height);
                vertex.color = color * pair.color;
                vh.AddVert(vertex);

                vertex.position = new Vector2(x, .5f * r.height);
                vertex.color = color * pair.color;
                vh.AddVert(vertex);

                int i2 = 2 + i * 2;

                vh.AddTriangle(i2 - 4, i2 - 3, i2 - 2);
                vh.AddTriangle(i2 - 3, i2 - 1, i2 - 2);
            }
        }
    }
}