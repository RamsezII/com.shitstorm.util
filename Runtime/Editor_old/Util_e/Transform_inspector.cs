#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace _EDITOR_
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Transform))]
    public class Transform_inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Transform rT = (Transform)target;

            if (GUILayout.Button(nameof(Util.GetPath)))
                Debug.Log(GUIUtility.systemCopyBuffer = rT.GetPath(true));
            if (GUILayout.Button(nameof(Util_e_OLD.LogTypes)))
                rT.LogTypes();
        }
    }
}
#endif