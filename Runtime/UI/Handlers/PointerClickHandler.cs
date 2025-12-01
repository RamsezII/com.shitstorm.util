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
            onPointerDown?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerDownHandler>()?.OnPointerDown(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerUpHandler>()?.OnPointerUp(eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerClickHandler>()?.OnPointerClick(eventData);
        }
    }
}