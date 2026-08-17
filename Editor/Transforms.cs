#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace _UTIL_.Editor
{
    static class Transforms
    {
        [MenuItem("CONTEXT/" + nameof(Transform) + "/" + nameof(LogLayer))]
        static void LogLayer(MenuCommand command)
        {
            var rt = (Transform)command.context;
            Debug.Log(rt.gameObject.layer, rt);
        }
    }
}
#endif