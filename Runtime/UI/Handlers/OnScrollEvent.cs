using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnScrollEvent : MonoBehaviour, IScrollHandler
    {
        public Action<PointerEventData> onScroll;
        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            onScroll?.Invoke(eventData);
        }
    }
}