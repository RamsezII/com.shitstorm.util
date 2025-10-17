using System;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnScrollEvent : PointerHandler, IScrollHandler
    {
        public Action<PointerEventData> onScroll;
        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            onScroll?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IScrollHandler>()?.OnScroll(eventData);
        }
    }
}