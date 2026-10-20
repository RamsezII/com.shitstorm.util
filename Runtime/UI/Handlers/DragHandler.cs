using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class DragHandler : PointerHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> onBeginDrag, onDrag, onEndDrag;
        public bool dragging;
        public Vector2 delta;

        //--------------------------------------------------------------------------------------------------------------

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;
            delta = eventData.delta;
            onBeginDrag?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IBeginDragHandler>()?.OnBeginDrag(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            delta = eventData.delta;
            onDrag?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IDragHandler>()?.OnDrag(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
            delta = Vector2.zero;
            onEndDrag?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IEndDragHandler>()?.OnEndDrag(eventData);
        }
    }
}