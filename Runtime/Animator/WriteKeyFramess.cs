#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

partial class Util_e
{
    public static void WriteFloatKeyframes(this AnimationClip clip, in string path, in Type type, in string property_name, in float value, in float time)
    {
        EditorCurveBinding binding = new()
        {
            path = path,
            propertyName = property_name,
            type = type,
        };

        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding) ?? new AnimationCurve();
        curve.AddKey(new Keyframe(time, value));
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    public static void WriteLocalRotationKeyframes(this AnimationClip clip, in Transform transform, in float time)
    {
        string path = AnimationUtility.CalculateTransformPath(transform, transform.GetComponentInParent<Animator>().transform);
        Type type = typeof(Transform);
        Quaternion q = transform.localRotation;

        WriteFloatKeyframes(clip, path, type, "m_LocalRotation.x", q.x, time);
        WriteFloatKeyframes(clip, path, type, "m_LocalRotation.y", q.y, time);
        WriteFloatKeyframes(clip, path, type, "m_LocalRotation.z", q.z, time);
        WriteFloatKeyframes(clip, path, type, "m_LocalRotation.w", q.w, time);
    }

    public static void WriteStructKeyframes<T>(this AnimationClip clip, in Component component, in FieldInfo struct_field, in T struct_value, in float time) => WriteStructKeyframes(
        clip: clip,
        path: AnimationUtility.CalculateTransformPath(component.transform, component.GetComponentInParent<Animator>().transform),
        type: component.GetType(),
        struct_field: struct_field,
        struct_value: struct_value,
        time: time,
        prefixe: null
        );

    public static void WriteStructKeyframes<T>(this AnimationClip clip, in string path, in Type type, in FieldInfo struct_field, in T struct_value, in float time, in string prefixe = null)
    {
        foreach (FieldInfo sub_field in struct_field.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            string property_name = $"{prefixe ?? string.Empty}{struct_field.Name}.{sub_field.Name}";
            object value = sub_field.GetValue(struct_value);

            switch (value)
            {
                case float f:
                    WriteFloatKeyframes(clip, path, type, property_name, f, time);
                    break;

                case Vector2 v2:
                    WriteFloatKeyframes(clip, path, type, property_name + ".x", v2.x, time);
                    WriteFloatKeyframes(clip, path, type, property_name + ".y", v2.y, time);
                    break;

                case Vector3 v3:
                    WriteFloatKeyframes(clip, path, type, property_name + ".x", v3.x, time);
                    WriteFloatKeyframes(clip, path, type, property_name + ".y", v3.y, time);
                    WriteFloatKeyframes(clip, path, type, property_name + ".z", v3.z, time);
                    break;

                case Vector4 v4:
                    WriteFloatKeyframes(clip, path, type, property_name + ".x", v4.x, time);
                    WriteFloatKeyframes(clip, path, type, property_name + ".y", v4.y, time);
                    WriteFloatKeyframes(clip, path, type, property_name + ".z", v4.z, time);
                    WriteFloatKeyframes(clip, path, type, property_name + ".w", v4.w, time);
                    break;

                default:
                    WriteStructKeyframes(clip, path, type, sub_field, value, time, $"{prefixe}.{property_name}.");
                    break;
            }
        }
    }
}
#endif