using System;
using UnityEngine.UI;

namespace _UTIL_
{
    public sealed class UI_Graphic : Graphic
    {
        public Action<VertexHelper> onPopulateMesh;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            onPopulateMesh?.Invoke(vh);
        }
    }
}