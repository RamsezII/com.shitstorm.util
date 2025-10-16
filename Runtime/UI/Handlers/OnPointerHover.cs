using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnPointerHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Action<PointerEventData> onEnter, onExit;
        public Action<PointerEventData, bool> onEnterExit;

        //--------------------------------------------------------------------------------------------------------------

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onEnter?.Invoke(eventData);
            onEnterExit?.Invoke(eventData, true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onExit?.Invoke(eventData);
            onEnterExit?.Invoke(eventData, false);
        }
    }
}