#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

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
            _editor = CreateEditor(target, editorType);
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
        if (target is not BlendTree btree)
            return;

        if (_editor != null)
            _editor.OnInspectorGUI();
        else
        {
            EditorGUILayout.HelpBox("Inspector interne introuvable, fallback basique.", MessageType.Warning);
            base.OnInspectorGUI();
        }

        EditorGUI.BeginChangeCheck();
        var newType = (BlendTreeType)EditorGUILayout.EnumPopup("Blend Type", btree.blendType);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(btree, "Change Blend Tree Type");
            btree.blendType = newType;
            EditorUtility.SetDirty(btree);
        }

        AnimatorController controller = null;

        List<AnimatorState> states = new();
        foreach (var obj in Selection.objects)
            if (obj is AnimatorState state)
                states.Add(state);

        HashSet<int> possible_layers = new();

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
            {
                controller = ctrl;
                for (int j = 0; j < controller.layers.Length; j++)
                    if (controller.layers[j].syncedLayerIndex < 0)
                    {
                        if (!possible_layers.Contains(j))
                            if (StateMachineContainsState(controller.layers[j].stateMachine, state))
                                possible_layers.Add(j);
                    }
                    else
                    {
                        int base_layer = controller.layers[j].syncedLayerIndex;
                        if (possible_layers.Contains(base_layer))
                            possible_layers.Add(j);
                        else if (StateMachineContainsState(controller.layers[base_layer].stateMachine, state))
                        {
                            possible_layers.Add(base_layer);
                            possible_layers.Add(j);
                        }
                    }
            }
            else
                return;
        }

        foreach (var layer_i in possible_layers)
            ShowButton(controller, states.ToArray(), btree, layer_i);
    }

    static void ShowButton(in AnimatorController controller, in AnimatorState[] states, in BlendTree btree, in int layer_i)
    {
        string action_name = $"Assign {states.Length} states on layer \"{controller.layers[layer_i].name}\" ({layer_i})";

        if (GUILayout.Button(action_name))
        {
            Undo.RecordObjects(states, action_name);

            AnimatorControllerLayer[] layers = controller.layers;
            AnimatorControllerLayer layer = layers[layer_i];

            for (int j = 0; j < states.Length; j++)
            {
                AnimatorState state = states[j];

                if (layer.syncedLayerIndex >= 0)
                    layer.SetOverrideMotion(state, btree);
                else
                    state.motion = btree;
            }

            if (layer.syncedLayerIndex >= 0)
            {
                layers[layer_i] = layer;
                controller.layers = layers;
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            Debug.Log($"Assigned BlendTree '{btree.name}' to {states.Length} state(s).");
        }
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
