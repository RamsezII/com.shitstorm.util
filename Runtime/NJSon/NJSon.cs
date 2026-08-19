using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace _UTIL_
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NJFieldAttribute : Attribute
    {
        public string Name { get; }

        public NJFieldAttribute(string name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NJTransformAttribute : Attribute
    {
        public string Name { get; }

        public NJTransformAttribute(string name = null)
        {
            Name = name;
        }
    }

    public static class NJSon
    {
        const BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        // -------------------------------------------------------------------------------------------------------------

        public static void WriteNJFields(object obj, JObject json)
        {
            foreach (var field in obj.GetType().GetFields(flags))
            {
                var attr = field.GetCustomAttribute<NJFieldAttribute>();
                if (attr == null)
                    continue;

                string name = attr.Name ?? field.Name;
                object value = field.GetValue(obj);

                json[name] = value == null
                    ? JValue.CreateNull()
                    : JToken.FromObject(value);
            }
        }

        public static void ReadNJFields(object obj, JObject json)
        {
            foreach (var field in obj.GetType().GetFields(flags))
            {
                var attr = field.GetCustomAttribute<NJFieldAttribute>();
                if (attr == null)
                    continue;

                string name = attr.Name ?? field.Name;

                if (!json.TryGetValue(name, out JToken token))
                    continue;

                SetValue(
                    field.FieldType,
                    token,
                    value => field.SetValue(obj, value));
            }
        }

        // -------------------------------------------------------------------------------------------------------------

        public static void WriteNJTransforms(in Transform root, object obj, JObject json)
        {
            foreach (var field in obj.GetType().GetFields(flags))
                if (field.FieldType == typeof(Transform) && field.GetValue(obj) is Transform transform)
                {
                    var attr = field.GetCustomAttribute<NJTransformAttribute>();
                    if (attr == null)
                        continue;

                    string name = attr.Name ?? field.Name;
                    json[name] = new JObject()
                    {
                        [nameof(transform.localPosition)] = JsonConvert.SerializeObject(transform.localPosition),
                        [nameof(transform.localEulerAngles)] = JsonConvert.SerializeObject(transform.localEulerAngles.SignedEulers()),
                        ["relativePath"] = transform.GetRelativePath(root),
                    };
                }
        }

        public static void ReadNJTransforms(in Transform root, object obj, JObject json)
        {
            foreach (var field in obj.GetType().GetFields(flags))
                if (field.FieldType == typeof(Transform))
                {
                    var attr = field.GetCustomAttribute<NJTransformAttribute>();
                    if (attr == null)
                        continue;

                    string name = attr.Name ?? field.Name;

                    if (json[name] is JObject jtfm)
                        if (jtfm.TryGetValue("relativePath", out var jpath))
                        {
                            Transform transform = root.ForceFind((string)jpath, false);

                            if (jtfm.TryGetValue(nameof(transform.localPosition), out var jpos))
                                transform.localPosition = jpos.ToObject<Vector3>();

                            if (jtfm.TryGetValue(nameof(transform.localEulerAngles), out var jeul))
                                transform.localEulerAngles = jeul.ToObject<Vector3>();

                            field.SetValue(obj, transform);
                        }
                }
        }

        // -------------------------------------------------------------------------------------------------------------

        static void SetValue(Type type, JToken token, Action<object> setter)
        {
            if (token.Type == JTokenType.Null)
            {
                // null autorisé uniquement pour références et Nullable<T>
                if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
                    setter(null);

                return;
            }

            setter(token.ToObject(type));
        }
    }
}