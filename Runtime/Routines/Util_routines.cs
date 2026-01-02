using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class Util
{
    public static IEnumerator<T> ExtractCallback<T>(this IEnumerator<T> eT, Action<T> onT)
    {
        while (eT.MoveNext())
            yield return eT.Current;
        onT(eT.Current);
    }

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

    public static IEnumerator<float> ESequence(params Action[] actions)
    {
        yield return 0;
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].Invoke();
            yield return (float)(1 + i) / actions.Length;
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

    public static IEnumerator<float> EWhile(this Func<bool> get_bool, Func<float> get_progress, Action on_while_done)
    {
        while (get_bool())
            if (get_progress == null)
                yield return get_progress?.Invoke() ?? 0;
        on_while_done();
    }

    public static IEnumerator<float> EWaitUntil(this Func<bool> condition, Func<float> progression, Action action)
    {
        while (!condition())
            if (progression == null)
                yield return progression?.Invoke() ?? 0;
        action();
    }
}