#if UNITY_EDITOR && DEFAULT_RTFMS
using UnityEditor;
using UnityEngine;

namespace _EDITOR_
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RectTransform))]
    public class RectTransform_inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RectTransform rT = (RectTransform)target;
            Transform T = rT;

            if (GUILayout.Button(nameof(Util.FillParent)))
                rT.FillParent();
            if (GUILayout.Button(nameof(Util.GetPath)))
                Debug.Log(GUIUtility.systemCopyBuffer = rT.GetPath(true));
            if (GUILayout.Button(nameof(Util_e_OLD.LogTypes)))
                rT.LogTypes();
        }
    }
}
#endif