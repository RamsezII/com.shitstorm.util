using UnityEngine;
using UnityEngine.EventSystems; // N'oubliez pas d'inclure cet espace de noms

public class MouseEventsExample : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // Cette fonction est appelée lorsqu'on survole l'élément avec la souris
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("La souris est entrée sur " + gameObject.name);
    }

    // Cette fonction est appelée lorsque la souris quitte l'élément
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("La souris a quitté " + gameObject.name);
    }

    // Cette fonction est appelée lorsque l'élément est cliqué
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("L'élément " + gameObject.name + " a été cliqué");
    }

    // Ajoutez d'autres méthodes pour d'autres événements de souris au besoin, comme OnPointerDown, OnPointerUp, etc.
}
