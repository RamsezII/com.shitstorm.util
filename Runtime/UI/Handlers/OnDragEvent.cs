using System;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnDragEvent : PointerHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> onBeginDrag, onDrag, onEndDrag;

        //--------------------------------------------------------------------------------------------------------------

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
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
            onEndDrag?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IEndDragHandler>()?.OnEndDrag(eventData);
        }
    }
}