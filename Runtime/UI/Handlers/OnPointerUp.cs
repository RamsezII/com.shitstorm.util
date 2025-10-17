using System;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnPointerUp : PointerHandler, IPointerUpHandler
    {
        public Action<PointerEventData> onUp;
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            onUp?.Invoke(eventData);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerUpHandler>()?.OnPointerUp(eventData);
        }
    }
}