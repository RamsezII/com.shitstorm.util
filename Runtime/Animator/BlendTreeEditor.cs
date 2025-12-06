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

        if (GUILayout.Button("Apply to selected Animator States"))
        {
            if (target is not BlendTree blendTree)
            {
                Debug.LogError($"{target} is not {typeof(BlendTree).FullName}");
                return;
            }

            List<AnimatorState> states = new();
            foreach (var obj in Selection.objects)
                if (obj is AnimatorState state)
                    states.Add(state);

            if (states.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    title: "No Animator State selected",
                    message: "Select one or more Animator States in the Animator window, then click the button again.",
                    ok: "Ok"
                );
                return;
            }

            Undo.RecordObjects(states.ToArray(), "Assign BlendTree to States");

            // On prépare à marquer les controllers comme dirty
            var dirty_controllers = new HashSet<AnimatorController>();

            for (int i = 0; i < states.Count; i++)
            {
                AnimatorState state = states[i];
                state.motion = blendTree;

                string path = AssetDatabase.GetAssetPath(state);
                if (!string.IsNullOrEmpty(path))
                {
                    var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                    if (controller != null)
                        dirty_controllers.Add(controller);
                }
            }

            foreach (var ctrl in dirty_controllers)
                EditorUtility.SetDirty(ctrl);

            AssetDatabase.SaveAssets();

            Debug.Log($"Assigned BlendTree '{blendTree.name}' to {states.Count} state(s).");
        }
    }
}
#endif
