using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class SelectionHandler : PointerHandler, ISelectHandler, IDeselectHandler
    {
        public Action<BaseEventData> onEnter, onExit;
        public Action<BaseEventData, bool> onEnterExit;
        [SerializeField] UnityEvent<bool> onSelection;

        //--------------------------------------------------------------------------------------------------------------

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            onEnter?.Invoke(eventData);
            onEnterExit?.Invoke(eventData, true);
            onSelection.Invoke(true);
            if (propagateToParent)
                transform.parent.GetComponentInParent<ISelectHandler>()?.OnSelect(eventData);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            onExit?.Invoke(eventData);
            onEnterExit?.Invoke(eventData, false);
            onSelection.Invoke(false);
            if (propagateToParent)
                transform.parent.GetComponentInParent<IDeselectHandler>()?.OnDeselect(eventData);
        }
    }
}