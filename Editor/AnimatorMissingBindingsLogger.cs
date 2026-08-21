using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace _UTIL_.Editor
{
    internal static class AnimatorMissingBindingsLogger
    {
        const string MenuPath = "CONTEXT/Animator/Log missing animation bindings";

        struct BindingInfo
        {
            public EditorCurveBinding binding;
            public bool isObjectReference;
        }

        [MenuItem(MenuPath, false, 20)]
        static void LogMissingAnimationBindings(MenuCommand command)
        {
            Animator animator = command.context as Animator;
            if (animator == null)
                return;

            if (!TryGetController(
                    animator.runtimeAnimatorController,
                    out AnimatorController controller,
                    out Dictionary<AnimationClip, AnimationClip> overrides))
            {
                Debug.LogWarning(
                    "[Animator bindings] '" + animator.name +
                    "' has no supported AnimatorController.",
                    animator);
                return;
            }

            int missingClipCount = 0;
            int missingBindingCount = 0;
            AnimatorControllerLayer[] layers = controller.layers;
            StringBuilder report = new StringBuilder();
            report.Append("Animator: ").AppendLine(animator.name)
                .Append("Controller: ").AppendLine(controller.name)
                .AppendLine();

            for (int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
            {
                AnimatorControllerLayer layer = layers[layerIndex];
                AnimatorStateMachine stateMachine = layer.syncedLayerIndex >= 0
                    ? layers[layer.syncedLayerIndex].stateMachine
                    : layer.stateMachine;

                List<AnimationClip> clips = new List<AnimationClip>();
                Dictionary<AnimationClip, List<string>> clipStates =
                    new Dictionary<AnimationClip, List<string>>();
                List<string> statesWithoutMotion = new List<string>();
                CollectStateMachine(
                    layer,
                    stateMachine,
                    layer.name,
                    overrides,
                    clips,
                    clipStates,
                    statesWithoutMotion);

                if (clips.Count == 0)
                    continue;

                Dictionary<string, BindingInfo> union =
                    new Dictionary<string, BindingInfo>(StringComparer.Ordinal);
                Dictionary<AnimationClip, HashSet<string>> clipBindings =
                    new Dictionary<AnimationClip, HashSet<string>>();

                for (int i = 0; i < clips.Count; ++i)
                {
                    AnimationClip clip = clips[i];
                    HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
                    clipBindings.Add(clip, keys);

                    AddBindings(
                        AnimationUtility.GetCurveBindings(clip),
                        false,
                        keys,
                        union);
                    AddBindings(
                        AnimationUtility.GetObjectReferenceCurveBindings(clip),
                        true,
                        keys,
                        union);
                }

                for (int i = 0; i < clips.Count; ++i)
                {
                    AnimationClip clip = clips[i];
                    HashSet<string> keys = clipBindings[clip];
                    List<BindingInfo> missing = new List<BindingInfo>();

                    foreach (KeyValuePair<string, BindingInfo> pair in union)
                    {
                        if (!keys.Contains(pair.Key))
                            missing.Add(pair.Value);
                    }

                    if (missing.Count == 0)
                        continue;

                    missing.Sort(CompareBindings);
                    ++missingClipCount;
                    missingBindingCount += missing.Count;

                    StringBuilder message = new StringBuilder();
                    message.Append("[Animator bindings] Layer ")
                        .Append(layerIndex + 1)
                        .Append(" '")
                        .Append(layer.name)
                        .Append("' (index ")
                        .Append(layerIndex)
                        .Append("), clip '")
                        .Append(clip.name)
                        .Append("' is missing ")
                        .Append(missing.Count)
                        .Append(" bindings.")
                        .Append("\nStates:");

                    List<string> statePaths = clipStates[clip];
                    for (int j = 0; j < statePaths.Count; ++j)
                        message.Append("\n - ").Append(statePaths[j]);

                    message.Append("\nMissing bindings:");

                    for (int j = 0; j < missing.Count; ++j)
                    {
                        EditorCurveBinding binding = missing[j].binding;
                        message.Append("\n - ")
                            .Append(binding.propertyName)
                            .Append(" | ")
                            .Append(binding.type != null ? binding.type.Name : "<unknown type>")
                            .Append(" | path: ")
                            .Append(string.IsNullOrEmpty(binding.path) ? "<root>" : binding.path);

                        if (missing[j].isObjectReference)
                            message.Append(" | object reference");
                    }

                    string assetPath = AssetDatabase.GetAssetPath(clip);
                    if (!string.IsNullOrEmpty(assetPath))
                        message.Append("\nAsset: ").Append(assetPath);

                    report.AppendLine(message.ToString()).AppendLine();
                }

                if (statesWithoutMotion.Count > 0 && union.Count > 0)
                {
                    StringBuilder message = new StringBuilder();
                    message.Append("[Animator bindings] Layer ")
                        .Append(layerIndex + 1)
                        .Append(" '")
                        .Append(layer.name)
                        .Append("' contains states with no animation clip:");

                    for (int i = 0; i < statesWithoutMotion.Count; ++i)
                        message.Append("\n - ").Append(statesWithoutMotion[i]);

                    report.AppendLine(message.ToString()).AppendLine();
                }
            }

            if (missingClipCount == 0)
            {
                report.AppendLine("No missing bindings found.");
            }
            else
            {
                report.Append("Summary: ")
                    .Append(missingBindingCount)
                    .Append(" missing binding/clip pairs across ")
                    .Append(missingClipCount)
                    .AppendLine(" clips.");
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputDirectory = Path.Combine(projectRoot, "home", "EditorTemp");
            Directory.CreateDirectory(outputDirectory);

            string outputPath = Path.Combine(
                outputDirectory,
                typeof(AnimatorMissingBindingsLogger).FullName + "." +
                SanitizeFileName(animator.name) + ".txt");
            File.WriteAllText(outputPath, report.ToString(), Encoding.UTF8);

            Application.OpenURL(outputPath);

            string consoleMessage =
                "[Animator bindings] '" + animator.name + "': " +
                missingBindingCount + " missing binding/clip pairs across " +
                missingClipCount + " clips. Report: " + outputPath;

            if (missingClipCount == 0)
                Debug.Log(consoleMessage, animator);
            else
                Debug.LogWarning(consoleMessage, animator);
        }

        static string SanitizeFileName(string value)
        {
            char[] invalidCharacters = Path.GetInvalidFileNameChars();
            StringBuilder result = new StringBuilder(value.Length);

            for (int i = 0; i < value.Length; ++i)
            {
                char character = value[i];
                result.Append(Array.IndexOf(invalidCharacters, character) >= 0
                    ? '_'
                    : character);
            }

            return result.ToString();
        }

        static bool TryGetController(
            RuntimeAnimatorController runtimeController,
            out AnimatorController controller,
            out Dictionary<AnimationClip, AnimationClip> overrides)
        {
            overrides = new Dictionary<AnimationClip, AnimationClip>();

            AnimatorOverrideController overrideController =
                runtimeController as AnimatorOverrideController;
            controller = overrideController != null
                ? overrideController.runtimeAnimatorController as AnimatorController
                : runtimeController as AnimatorController;

            if (controller == null)
                return false;

            if (overrideController == null)
                return true;

            List<KeyValuePair<AnimationClip, AnimationClip>> pairs =
                new List<KeyValuePair<AnimationClip, AnimationClip>>(
                    overrideController.overridesCount);
            overrideController.GetOverrides(pairs);

            for (int i = 0; i < pairs.Count; ++i)
            {
                AnimationClip original = pairs[i].Key;
                if (original == null)
                    continue;

                overrides[original] = pairs[i].Value != null
                    ? pairs[i].Value
                    : original;
            }

            return true;
        }

        static void CollectStateMachine(
            AnimatorControllerLayer layer,
            AnimatorStateMachine stateMachine,
            string statePath,
            Dictionary<AnimationClip, AnimationClip> overrides,
            List<AnimationClip> clips,
            Dictionary<AnimationClip, List<string>> clipStates,
            List<string> statesWithoutMotion)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; ++i)
            {
                AnimatorState state = states[i].state;
                Motion motion = layer.syncedLayerIndex >= 0
                    ? layer.GetOverrideMotion(state)
                    : state.motion;

                if (motion == null && layer.syncedLayerIndex >= 0)
                    motion = state.motion;

                List<AnimationClip> stateClips = new List<AnimationClip>();
                CollectClips(motion, overrides, stateClips);

                if (stateClips.Count == 0)
                {
                    statesWithoutMotion.Add(statePath + "/" + state.name);
                    continue;
                }

                string fullStatePath = statePath + "/" + state.name;
                for (int j = 0; j < stateClips.Count; ++j)
                {
                    AnimationClip clip = stateClips[j];
                    if (!clipStates.TryGetValue(clip, out List<string> statePaths))
                    {
                        statePaths = new List<string>();
                        clipStates.Add(clip, statePaths);
                        clips.Add(clip);
                    }

                    if (!statePaths.Contains(fullStatePath))
                        statePaths.Add(fullStatePath);
                }
            }

            ChildAnimatorStateMachine[] children = stateMachine.stateMachines;
            for (int i = 0; i < children.Length; ++i)
            {
                AnimatorStateMachine child = children[i].stateMachine;
                CollectStateMachine(
                    layer,
                    child,
                    statePath + "/" + child.name,
                    overrides,
                    clips,
                    clipStates,
                    statesWithoutMotion);
            }
        }

        static void CollectClips(
            Motion motion,
            Dictionary<AnimationClip, AnimationClip> overrides,
            List<AnimationClip> result)
        {
            if (motion is AnimationClip clip)
            {
                if (overrides.TryGetValue(clip, out AnimationClip overrideClip))
                    clip = overrideClip;

                if (clip != null && !result.Contains(clip))
                    result.Add(clip);
                return;
            }

            if (!(motion is BlendTree blendTree))
                return;

            ChildMotion[] children = blendTree.children;
            for (int i = 0; i < children.Length; ++i)
                CollectClips(children[i].motion, overrides, result);
        }

        static void AddBindings(
            EditorCurveBinding[] bindings,
            bool isObjectReference,
            HashSet<string> clipKeys,
            Dictionary<string, BindingInfo> union)
        {
            for (int i = 0; i < bindings.Length; ++i)
            {
                EditorCurveBinding binding = bindings[i];
                string key = GetBindingKey(binding, isObjectReference);
                clipKeys.Add(key);

                if (!union.ContainsKey(key))
                {
                    union.Add(key, new BindingInfo
                    {
                        binding = binding,
                        isObjectReference = isObjectReference
                    });
                }
            }
        }

        static string GetBindingKey(
            EditorCurveBinding binding,
            bool isObjectReference)
        {
            return (isObjectReference ? "O" : "F") + "\n" +
                binding.path + "\n" +
                (binding.type != null ? binding.type.FullName : string.Empty) + "\n" +
                binding.propertyName;
        }

        static int CompareBindings(BindingInfo left, BindingInfo right)
        {
            int propertyComparison = string.Compare(
                left.binding.propertyName,
                right.binding.propertyName,
                StringComparison.Ordinal);
            if (propertyComparison != 0)
                return propertyComparison;

            return string.Compare(
                left.binding.path,
                right.binding.path,
                StringComparison.Ordinal);
        }
    }
}
