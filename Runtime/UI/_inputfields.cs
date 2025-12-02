using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

partial class Util
{
    public static bool IsInputFieldFocused()
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        if (obj == null)
            return false;
        return obj.GetComponent<TMP_InputField>() != null;
    }
}