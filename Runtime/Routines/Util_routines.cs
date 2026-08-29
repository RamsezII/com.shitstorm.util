using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class Util
{
    public static IEnumerator<float> EWaitForFrames(this int frames, string name, UnityEngine.Object context, Action action)
    {
        for (int i = 0; i < frames; i++)
            yield return (float)i / frames;

        try
        {
            action();
        }
        catch (Exception e)
        {
            Debug.LogError($"ewait \"{name}\" ({frames} frames) failed", context);
            throw e;
        }
    }

    public static IEnumerator<float> EWaitForSeconds(this float seconds, bool scaled, Action action)
    {
        float timer = seconds;
        while (timer > 0)
        {
            if (scaled)
                timer -= Time.deltaTime;
            else
                timer -= Time.unscaledDeltaTime;
            yield return Mathf.Clamp01(1 - timer / seconds);
        }
        action?.Invoke();
    }
}