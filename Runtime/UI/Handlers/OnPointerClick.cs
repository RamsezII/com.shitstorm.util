using System;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnPointerClick : PointerHandler, IPointerClickHandler
    {
        public Action<PointerEventData> onClick;
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerClickHandler>()?.OnPointerClick(eventData);
        }
    }
}