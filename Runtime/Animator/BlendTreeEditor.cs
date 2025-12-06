#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(BlendTree))]
public class BlendTreeEditor : Editor
{
    Editor _editor;

    //----------------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        Type editorType = typeof(Editor).Assembly.GetType("BlendTreeInspector, UnityEditor");

        if (editorType == null)
            foreach (var t in typeof(Editor).Assembly.GetTypes())
                if (t.Name == "BlendTreeInspector")
                    editorType = t;

        if (editorType != null)
            _editor = CreateEditor(targets, editorType);
        else
            Debug.LogError("Impossible de trouver BlendTreeInspector in UnityEditor");
    }

    private void OnDisable()
    {
        if (_editor != null)
            DestroyImmediate(_editor);
    }

    //----------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI()
    {
        if (_editor != null)
            _editor.OnInspectorGUI();
        else
        {
            EditorGUILayout.HelpBox("Inspector interne introuvable, fallback basique.", MessageType.Warning);
            base.OnInspectorGUI();
        }

        if (target is not BlendTree blendTree)
            return;

        AnimatorController controller = null;

        List<AnimatorState> states = new();
        foreach (var obj in Selection.objects)
            if (obj is AnimatorState state)
                states.Add(state);

        if (states.Count == 0)
            return;

        for (int i = 0; i < states.Count; i++)
        {
            AnimatorState state = states[i];
            string path = AssetDatabase.GetAssetPath(state);

            if (string.IsNullOrWhiteSpace(path))
                return;

            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null || ctrl == controller)
                controller = ctrl;
            else
                return;
        }

        bool swap = false;

        for (int i = 0; i < controller.layers.Length; ++i)
        {
            AnimatorControllerLayer layer = controller.layers[i];
            string action_name = $"Assign states (layer {i} \"{layer.name}\")";

            if (GUILayout.Button(action_name))
            {
                Undo.RecordObjects(states.ToArray(), action_name);

                for (int j = 0; j < states.Count; j++)
                {
                    AnimatorState state = states[j];

                    if (swap)
                        if (layer.syncedLayerIndex < 0)
                            state.motion = blendTree;
                        else
                            layer.SetOverrideMotion(state, blendTree);
                }

                if (swap)
                    if (layer.syncedLayerIndex < 0)
                        controller.layers[i] = layer;

                if (swap)
                {
                    EditorUtility.SetDirty(controller);
                    AssetDatabase.SaveAssets();
                }

                Debug.Log($"Assigned BlendTree '{blendTree.name}' to {states.Count} state(s).");
            }
        }
    }

    static bool TryGetLayerIndex(in AnimatorController controller, in AnimatorState target, out int layerIndex)
    {
        for (int i = 0; i < controller.layers.Length; i++)
            if (StateMachineContainsState(controller.layers[i].stateMachine, target))
            {
                layerIndex = i;
                return true;
            }
        layerIndex = -1; // pas trouvé
        return false;
    }

    static bool StateMachineContainsState(in AnimatorStateMachine stateMachine, in AnimatorState target)
    {
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            ChildAnimatorState subState = stateMachine.states[i];
            if (subState.state == target)
                return true;
        }

        for (int i = 0; i < stateMachine.stateMachines.Length; i++)
        {
            ChildAnimatorStateMachine subStateMachine = stateMachine.stateMachines[i];
            if (StateMachineContainsState(subStateMachine.stateMachine, target))
                return true;
        }

        return false;
    }
}
#endif
