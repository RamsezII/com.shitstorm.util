#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace _UTIL_.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    [CanEditMultipleObjects]
    sealed class ValueNotifierMonoBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            ValueNotifierInspectorGUI.Draw(targets);
            DrawDefaultInspector();
        }

        public override bool RequiresConstantRepaint()
        {
            return EditorApplication.isPlaying && target != null && ValueNotifierInspectorGUI.HasNotifiers(target.GetType());
        }
    }

    [CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
    [CanEditMultipleObjects]
    sealed class ValueNotifierScriptableObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            ValueNotifierInspectorGUI.Draw(targets);
            DrawDefaultInspector();
        }

        public override bool RequiresConstantRepaint()
        {
            return EditorApplication.isPlaying && target != null && ValueNotifierInspectorGUI.HasNotifiers(target.GetType());
        }
    }

    static class ValueNotifierInspectorGUI
    {
        const BindingFlags notifier_field_flags =
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        static readonly Dictionary<Type, FieldInfo[]> notifier_fields = new();

        //----------------------------------------------------------------------------------------------------------

        public static bool HasNotifiers(Type inspected_type)
        {
            return GetNotifierFields(inspected_type).Length > 0;
        }

        public static void Draw(UnityEngine.Object[] targets)
        {
            if (targets == null || targets.Length == 0 || targets[0] == null)
                return;

            Type inspected_type = targets[0].GetType();
            FieldInfo[] fields = GetNotifierFields(inspected_type);
            if (fields.Length == 0)
                return;

            string foldout_key = $"{nameof(ValueNotifierInspectorGUI)}.{inspected_type.AssemblyQualifiedName}";
            bool expanded = SessionState.GetBool(foldout_key, true);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                expanded = EditorGUILayout.Foldout(expanded, $"ValueNotifiers ({fields.Length})", true, EditorStyles.foldoutHeader);
                SessionState.SetBool(foldout_key, expanded);

                if (!expanded)
                    return;

                EditorGUI.indentLevel++;
                foreach (FieldInfo field in fields)
                    DrawNotifier(targets, field);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2f);
        }

        static void DrawNotifier(UnityEngine.Object[] targets, FieldInfo field)
        {
            ValueNotifier first_notifier = GetNotifier(targets[0], field);
            object first_value = GetValue(first_notifier);
            Type value_type = GetValueType(first_notifier, field);
            bool mixed = false;

            if (!field.IsStatic)
                for (int i = 1; i < targets.Length; i++)
                {
                    ValueNotifier notifier = GetNotifier(targets[i], field);
                    object value = GetValue(notifier);
                    if (!Equals(first_value, value))
                    {
                        mixed = true;
                        break;
                    }
                }

            string field_name = ObjectNames.NicifyVariableName(field.Name);
            if (field.IsStatic)
                field_name += " (static)";

            GUIContent label = new(field_name, GetTooltip(field, first_notifier));

            bool previous_show_mixed_value = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixed;

            if (TryDrawEditableValue(label, value_type, first_value, first_notifier, out object new_value))
                if (field.IsStatic)
                    SetValue(first_notifier, new_value);
                else
                    foreach (UnityEngine.Object target in targets)
                        SetValue(GetNotifier(target, field), new_value);

            EditorGUI.showMixedValue = previous_show_mixed_value;
        }

        static bool TryDrawEditableValue(GUIContent label, Type value_type, object value, ValueNotifier notifier, out object new_value)
        {
            bool editable = true;

            EditorGUI.BeginChangeCheck();

            if (value_type == typeof(bool))
                new_value = EditorGUILayout.Toggle(label, value is bool bool_value && bool_value);
            else if (value_type == typeof(int))
                new_value = EditorGUILayout.IntField(label, value is int int_value ? int_value : 0);
            else if (value_type == typeof(byte))
                new_value = (byte)Mathf.Clamp(EditorGUILayout.IntField(label, value is byte byte_value ? byte_value : 0), byte.MinValue, byte.MaxValue);
            else if (value_type == typeof(float))
                new_value = EditorGUILayout.FloatField(label, value is float float_value ? float_value : 0f);
            else if (value_type == typeof(double))
                new_value = EditorGUILayout.DoubleField(label, value is double double_value ? double_value : 0d);
            else if (value_type == typeof(string))
                new_value = EditorGUILayout.TextField(label, value as string ?? string.Empty);
            else if (value_type.IsEnum)
            {
                Enum enum_value = value as Enum ?? (Enum)Enum.ToObject(value_type, 0);
                new_value = value_type.IsDefined(typeof(FlagsAttribute), false)
                    ? EditorGUILayout.EnumFlagsField(label, enum_value)
                    : EditorGUILayout.EnumPopup(label, enum_value);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(value_type))
                new_value = EditorGUILayout.ObjectField(label, value as UnityEngine.Object, value_type, true);
            else if (value_type == typeof(Color))
                new_value = EditorGUILayout.ColorField(label, value is Color color ? color : default);
            else if (value_type == typeof(Vector2))
                new_value = EditorGUILayout.Vector2Field(label, value is Vector2 vector2 ? vector2 : default);
            else if (value_type == typeof(Vector3))
                new_value = EditorGUILayout.Vector3Field(label, value is Vector3 vector3 ? vector3 : default);
            else if (value_type == typeof(Vector4))
                new_value = EditorGUILayout.Vector4Field(label, value is Vector4 vector4 ? vector4 : default);
            else if (value_type == typeof(Vector2Int))
                new_value = EditorGUILayout.Vector2IntField(label, value is Vector2Int vector2_int ? vector2_int : default);
            else if (value_type == typeof(Vector3Int))
                new_value = EditorGUILayout.Vector3IntField(label, value is Vector3Int vector3_int ? vector3_int : default);
            else if (value_type == typeof(Quaternion))
            {
                Quaternion quaternion = value is Quaternion quaternion_value ? quaternion_value : default;
                Vector4 vector = EditorGUILayout.Vector4Field(label, new(quaternion.x, quaternion.y, quaternion.z, quaternion.w));
                new_value = new Quaternion(vector.x, vector.y, vector.z, vector.w);
            }
            else
            {
                editable = false;
                new_value = value;
                EditorGUILayout.LabelField(label, FormatValue(value, notifier));
            }

            bool changed = EditorGUI.EndChangeCheck();
            return editable && changed;
        }

        //----------------------------------------------------------------------------------------------------------

        static FieldInfo[] GetNotifierFields(Type inspected_type)
        {
            if (notifier_fields.TryGetValue(inspected_type, out FieldInfo[] fields))
                return fields;

            List<FieldInfo> result = new();

            for (Type type = inspected_type; type != null && type != typeof(UnityEngine.Object); type = type.BaseType)
                foreach (FieldInfo field in type.GetFields(notifier_field_flags))
                    if (typeof(ValueNotifier).IsAssignableFrom(field.FieldType))
                        result.Add(field);

            fields = result.ToArray();
            notifier_fields.Add(inspected_type, fields);
            return fields;
        }

        static ValueNotifier GetNotifier(UnityEngine.Object target, FieldInfo field)
        {
            try
            {
                return field.GetValue(field.IsStatic ? null : target) as ValueNotifier;
            }
            catch
            {
                return null;
            }
        }

        static object GetValue(ValueNotifier notifier)
        {
            if (notifier == null)
                return null;

            PropertyInfo value_property = notifier.GetType().GetProperty(
                nameof(ValueNotifier<int>.Value),
                BindingFlags.Instance | BindingFlags.Public);

            try
            {
                return value_property?.GetValue(notifier);
            }
            catch
            {
                return null;
            }
        }

        static Type GetValueType(ValueNotifier notifier, FieldInfo field)
        {
            Type type = notifier?.GetType() ?? field.FieldType;

            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueNotifier<>))
                    return type.GetGenericArguments()[0];

                type = type.BaseType;
            }

            return typeof(object);
        }

        static void SetValue(ValueNotifier notifier, object value)
        {
            if (notifier == null)
                return;

            try
            {
                FindField(notifier.GetType(), nameof(ValueNotifier<int>._value))?.SetValue(notifier, value);
            }
            catch
            {
            }
        }

        static string GetTooltip(FieldInfo field, ValueNotifier notifier)
        {
            string tooltip = field.FieldType.FullName;
            if (notifier == null)
                return tooltip + "\nnotifier: null";

            Type notifier_type = notifier.GetType();
            FieldInfo old_field = FindField(notifier_type, nameof(ValueNotifier<int>.old));
            FieldInfo changed_field = FindField(notifier_type, nameof(ValueNotifier<int>.changed));
            FieldInfo frame_field = FindField(notifier_type, nameof(ValueNotifier<int>.last_frame));

            object old_value = old_field?.GetValue(notifier);
            object changed = changed_field?.GetValue(notifier);
            object last_frame = frame_field?.GetValue(notifier);

            return $"{tooltip}\nold: {FormatValue(old_value)}\nchanged: {changed}\nlast frame: {last_frame}\ndisposed: {notifier._disposed}";
        }

        static FieldInfo FindField(Type type, string field_name)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(
                    field_name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }

        static string FormatValue(object value, ValueNotifier notifier = null)
        {
            if (notifier == null && value == null)
                return "null";
            if (notifier != null && value == null)
                return notifier._disposed ? "null (disposed)" : "null";

            string result = value switch
            {
                Type type => type.FullName,
                string text => text,
                _ => value.ToString()
            };

            if (notifier != null && notifier._disposed)
                result += " (disposed)";

            return result;
        }
    }
}
#endif
