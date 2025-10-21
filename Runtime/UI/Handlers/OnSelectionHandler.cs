using System;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnSelectionHandler : PointerHandler, ISelectHandler, IDeselectHandler
    {
        public Action<BaseEventData> onEnter, onExit;
        public Action<BaseEventData, bool> onEnterExit;

        //--------------------------------------------------------------------------------------------------------------

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            onEnter?.Invoke(eventData);
            onEnterExit?.Invoke(eventData, true);
            if (propagateToParent)
                transform.parent.GetComponentInParent<ISelectHandler>()?.OnSelect(eventData);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            onExit?.Invoke(eventData);
            onEnterExit?.Invoke(eventData, false);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IDeselectHandler>()?.OnDeselect(eventData);
        }
    }
}