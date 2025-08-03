#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace _UTIL_
{
    [CustomPropertyDrawer(typeof(ShowPropertyAttribute))]
    public class ShowPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowPropertyAttribute attr = (ShowPropertyAttribute)attribute;
            Object target = property.serializedObject.targetObject;

            // Récupérer la propriété via Reflection
            PropertyInfo propertyInfo = target.GetType().GetProperty(attr.propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (propertyInfo != null)
            {
                object value = propertyInfo.GetValue(target);
                // Afficher la valeur dans l'inspecteur
                EditorGUI.LabelField(position, attr.propertyName, value != null ? value.ToString() : "null");
            }
            else
                EditorGUI.LabelField(position, attr.propertyName, "⚠️ Propriété introuvable");
        }
    }
}
#endif