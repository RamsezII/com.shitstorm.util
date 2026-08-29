using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace _UTIL_
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NJTransformAttribute : Attribute
    {
    }

    public static partial class NJSon
    {
        public const BindingFlags BindingFlagsALL =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static;

        //----------------------------------------------------------------------------------------------------------

        public static void WriteFields<T>(this JObject jobj, in object target = null, in Type type = null) where T : Attribute
        {
            foreach (var field in (type ?? target.GetType()).GetFields(BindingFlagsALL))
            {
                var attr = field.GetCustomAttribute<T>();
                if (attr == null)
                    continue;

                object value = field.GetValue(target);
                jobj[field.Name] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
            }
        }

        public static void ReadFields<T>(this JObject jobj, in object target = null, in Type type = null) where T : Attribute
        {
            foreach (var field in (type ?? target.GetType()).GetFields(BindingFlagsALL))
            {
                var attr = field.GetCustomAttribute<T>();
                if (attr == null)
                    continue;

                if (!jobj.TryGetValue(field.Name, out JToken token))
                    continue;

                if (token.Type != JTokenType.Null)
                    field.SetValue(target, token.ToObject(field.FieldType));
                else
                {
                    // null autorisé uniquement pour références et Nullable<T>
                    if (!field.FieldType.IsValueType || Nullable.GetUnderlyingType(field.FieldType) != null)
                        field.SetValue(target, null);
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------------

        public static void WriteNJTransforms(this JObject jobj, object target, in Transform root)
        {
            foreach (var field in target.GetType().GetFields(BindingFlagsALL))
                if (field.FieldType == typeof(Transform) && field.GetValue(target) is Transform transform)
                {
                    var attr = field.GetCustomAttribute<NJTransformAttribute>();
                    if (attr == null)
                        continue;

                    jobj[field.Name] = new JObject()
                    {
                        [nameof(transform.localPosition)] = JsonConvert.SerializeObject(transform.localPosition),
                        [nameof(transform.localEulerAngles)] = JsonConvert.SerializeObject(transform.localEulerAngles.SignedEulers()),
                        [nameof(transform.localScale)] = JsonConvert.SerializeObject(transform.localScale),
                        ["relativePath"] = transform.GetRelativePath(root),
                    };
                }
        }

        public static void ReadNJTransforms(this JObject jobj, object target, in Transform root)
        {
            foreach (var field in target.GetType().GetFields(BindingFlagsALL))
                if (field.FieldType == typeof(Transform))
                {
                    var attr = field.GetCustomAttribute<NJTransformAttribute>();
                    if (attr == null)
                        continue;

                    if (jobj[field.Name] is JObject jtfm)
                        if (jtfm.TryGetValue("relativePath", out var jpath))
                        {
                            Transform transform = root.ForceFind((string)jpath, false);

                            if (jtfm.TryGetValue(nameof(transform.localPosition), out var jpos))
                                transform.localPosition = jpos.ToObject<Vector3>();

                            if (jtfm.TryGetValue(nameof(transform.localEulerAngles), out var jeul))
                                transform.localEulerAngles = jeul.ToObject<Vector3>();

                            if (jtfm.TryGetValue(nameof(transform.localScale), out var jscale))
                                transform.localScale = jscale.ToObject<Vector3>();

                            field.SetValue(target, transform);
                        }
                }
        }
    }
}