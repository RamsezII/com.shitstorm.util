#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

partial class Util
{
    public static bool TryGetCurrentAnimationClip(out AnimationClip clip, out float time)
    {
        clip = null;
        time = 0;

        AnimationWindow anim_window = EditorWindow.GetWindow<AnimationWindow>();
        if (anim_window == null)
        {
            Debug.LogWarning("AnimationWindow not open!");
            return false;
        }

        clip = anim_window.animationClip;
        time = anim_window.time;
        if (clip == null)
        {
            Debug.LogWarning("No animation clip selected!");
            return false;
        }

        return true;
    }
}
#endif
