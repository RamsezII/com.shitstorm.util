using System;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnPointerEnterExit : PointerHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Action<PointerEventData> onEnter, onExit;
        public Action<PointerEventData, bool> onEnterExit;

        //--------------------------------------------------------------------------------------------------------------

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onEnter?.Invoke(eventData);
            onEnterExit?.Invoke(eventData, true);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerEnterHandler>()?.OnPointerEnter(eventData);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onExit?.Invoke(eventData);
            onEnterExit?.Invoke(eventData, false);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IPointerExitHandler>()?.OnPointerExit(eventData);
        }
    }
}