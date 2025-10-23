using System;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnPointerHover : PointerHandler, IPointerMoveHandler
    {
        public Action<PointerEventData> onMove;

        //--------------------------------------------------------------------------------------------------------------

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            onMove?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerExitHandler>()?.OnPointerExit(eventData);
        }
    }
}