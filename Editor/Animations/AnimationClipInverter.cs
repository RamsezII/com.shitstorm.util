#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace _UTIL_.Editor
{
    static class AnimationClipTools
    {
        const string button_name = "Assets/" + nameof(_UTIL_) + "/" + nameof(InvertAllFloats);

        //----------------------------------------------------------------------------------------------------------

        [MenuItem(button_name, isValidateFunction: true)]
        static bool Validate() => Selection.activeObject is AnimationClip;

        [MenuItem(button_name)]
        static void InvertAllFloats()
        {
            if (Selection.activeObject is not AnimationClip clip)
                return;

            Undo.RegisterCompleteObjectUndo(clip, "Invert Animation Floats");

            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);

                for (int i = 0; i < curve.length; i++)
                {
                    var key = curve[i];

                    key.value = -key.value;
                    key.inTangent = -key.inTangent;
                    key.outTangent = -key.outTangent;

                    curve.MoveKey(i, key);
                }

                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }

            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif