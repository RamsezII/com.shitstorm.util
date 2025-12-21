using System;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class DragHandler : PointerHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> onBeginDrag, onDrag, onEndDrag;
        public bool dragging;

        //--------------------------------------------------------------------------------------------------------------

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;
            onBeginDrag?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IBeginDragHandler>()?.OnBeginDrag(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IDragHandler>()?.OnDrag(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
            onEndDrag?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IEndDragHandler>()?.OnEndDrag(eventData);
        }
    }
}