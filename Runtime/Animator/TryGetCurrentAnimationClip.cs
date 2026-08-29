#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

partial class Util
{
    public static bool TryGetCurrentAnimationClip(out AnimationClip clip, out float time)
    {
        clip = null;
        time = 0;

        Type anim_window_type = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimationWindow");
        if (anim_window_type == null)
        {
            Debug.LogWarning("UnityEditor.AnimationWindow type not found!");
            return false;
        }

        EditorWindow anim_window = EditorWindow.GetWindow(anim_window_type);
        if (anim_window == null)
        {
            Debug.LogWarning("AnimationWindow not open!");
            return false;
        }

        PropertyInfo anim_window_state_property = anim_window_type.GetProperty("animEditor", BindingFlags.NonPublic | BindingFlags.Instance);
        if (anim_window_state_property == null)
        {
            Debug.LogWarning("Property 'animEditor' not found on AnimationWindow!");
            return false;
        }

        object anim_window_state = anim_window_state_property.GetValue(anim_window, null);
        if (anim_window_state == null)
        {
            Debug.LogWarning("animEditor is null!");
            return false;
        }
        Type anim_window_state_type = anim_window_state.GetType();

        PropertyInfo state_property = anim_window_state_type.GetProperty("state", BindingFlags.Public | BindingFlags.Instance);
        if (state_property == null)
        {
            Debug.LogWarning("Property 'state' not found!");
            return false;
        }

        object state = state_property.GetValue(anim_window_state, null);
        if (state == null)
        {
            Debug.LogWarning("'state' property is null!");
            return false;
        }
        Type state_type = state.GetType();

        PropertyInfo current_time_property = state_type.GetProperty("currentTime", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (current_time_property == null)
        {
            Debug.LogWarning("Property 'currentTime' not found!");
            return false;
        }
        time = (float)current_time_property.GetValue(state, null);

        PropertyInfo clip_property =
            state_type.GetProperty("activeAnimationClip", BindingFlags.Public | BindingFlags.Instance)
            ?? state_type.GetProperty("animationClip", BindingFlags.Public | BindingFlags.Instance);

        if (clip_property == null)
        {
            Debug.LogWarning("No animation clip property found (tried 'activeAnimationClip' and 'animationClip')");
            return false;
        }

        clip = clip_property.GetValue(state, null) as AnimationClip;
        if (clip == null)
        {
            Debug.LogWarning("No animation clip selected!");
            return false;
        }

        return true;
    }
}
#endif