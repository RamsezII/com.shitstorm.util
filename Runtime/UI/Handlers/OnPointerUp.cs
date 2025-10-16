using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnPointerUp : MonoBehaviour, IPointerUpHandler
    {
        public Action<PointerEventData> onUp;
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            onUp?.Invoke(eventData);
        }
    }
}