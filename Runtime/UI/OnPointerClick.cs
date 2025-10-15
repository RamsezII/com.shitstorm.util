using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _UTIL_
{
    public sealed class OnPointerClick : MonoBehaviour, IPointerClickHandler
    {
        public Action<PointerEventData> onClick;
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(eventData);
        }
    }
}