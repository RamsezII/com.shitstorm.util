#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace _UTIL_.Editor
{
    static class RectTransforms
    {
        [MenuItem("CONTEXT/" + nameof(RectTransform) + "/" + nameof(LogSize))]
        static void LogSize(MenuCommand command)
        {
            RectTransform rt = (RectTransform)command.context;
            Debug.Log(rt.rect.size);
        }
    }
}
#endif