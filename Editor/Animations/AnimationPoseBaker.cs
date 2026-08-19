#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace _UTIL_.Editor
{
    static class AnimationPoseBaker
    {
        const string button_name = "CONTEXT/" + nameof(Animator) + "/" + nameof(_UTIL_) + "/" + nameof(BakeCurrentPose);

        //----------------------------------------------------------------------------------------------------------

        [MenuItem(button_name, false, 20)]
        static void BakeCurrentPose(MenuCommand command)
        {
            Animator animator = (Animator)command.context;

            AnimationWindow window = Resources
                .FindObjectsOfTypeAll<AnimationWindow>()
                .FirstOrDefault(w => w.animationClip != null);

            if (window == null)
            {
                Debug.LogWarning("No Animation Window with an open clip.", animator);
                return;
            }

            AnimationClip clip = window.animationClip;

            Undo.RegisterCompleteObjectUndo(clip, "Bake Current Pose");

            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                // Évite d'écraser d'autres propriétés animées :
                // lights, scripts, blendshapes, etc.
                if (binding.type != typeof(Transform))
                    continue;

                if (!AnimationUtility.GetFloatValue(animator.gameObject, binding, out float value))
                    continue;

                AnimationCurve curve =
                    AnimationUtility.GetEditorCurve(clip, binding);

                for (int i = 0; i < curve.length; i++)
                {
                    Keyframe key = curve[i];

                    key.value = value;
                    key.inTangent = 0f;
                    key.outTangent = 0f;

                    curve.MoveKey(i, key);
                }

                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }

            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            window.Repaint();

            Debug.Log(
                $"Baked current pose of '{animator.name}' into '{clip.name}'.",
                clip);
        }

        [MenuItem(button_name, true)]
        static bool ValidateBakeCurrentPose()
        {
            return Selection.activeGameObject != null &&
                   Resources.FindObjectsOfTypeAll<AnimationWindow>()
                       .Any(w => w.animationClip != null);
        }
    }
}
#endif