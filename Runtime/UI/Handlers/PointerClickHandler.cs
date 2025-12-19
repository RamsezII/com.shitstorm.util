using System;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class PointerClickHandler : PointerHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Action<PointerEventData> onClick, onPointerDown, onPointerUp;

        //--------------------------------------------------------------------------------------------------------------

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.dragging)
                return;

            if (onPointerDown != null)
                onPointerDown.Invoke(eventData);
            else if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerDownHandler>()?.OnPointerDown(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.dragging)
                return;

            if (onPointerUp != null)
                onPointerUp.Invoke(eventData);
            else if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerUpHandler>()?.OnPointerUp(eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging)
                return;

            if (onClick != null)
                onClick.Invoke(eventData);
            else if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerClickHandler>()?.OnPointerClick(eventData);
        }
    }
}