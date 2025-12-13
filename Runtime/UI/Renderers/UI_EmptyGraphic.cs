using UnityEngine;
using UnityEngine.UI;

namespace _UTIL_
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UI_EmptyGraphic : Graphic
    {
        [ContextMenu(nameof(SetVerticesDirty))]
        public override void SetVerticesDirty() => base.SetVerticesDirty();
        protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();
    }
}