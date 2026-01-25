#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace _UTIL_e
{
    public sealed class TextPrompt : EditorWindow
    {
        string value = "";
        string label;
        Action<string> onValidate;

        //--------------------------------------------------------------------------------------------------------------

        public static void Show(in string title, in string defaultValue, in Action<string> onValidate)
        {
            var w = CreateInstance<TextPrompt>();
            w.titleContent = new GUIContent(title);
            w.label = "nom :";
            w.value = defaultValue;
            w.onValidate = onValidate;
            w.minSize = new Vector2(300, 70);
            w.ShowUtility(); // fenêtre modale légère
        }

        //--------------------------------------------------------------------------------------------------------------

        void OnGUI()
        {
            GUILayout.Label(label, EditorStyles.boldLabel);
            GUI.SetNextControlName("name");
            value = EditorGUILayout.TextField(value);

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Annuler"))
                Close();

            GUI.enabled = !string.IsNullOrWhiteSpace(value);
            if (GUILayout.Button("OK"))
            {
                string trim = value.Trim();
                onValidate?.Invoke(trim);
                Close();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            EditorGUI.FocusTextInControl("name");
        }
    }
}
#endif