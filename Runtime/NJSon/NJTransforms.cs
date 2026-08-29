using _UTIL_;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using UnityEngine;

namespace _UTIL_
{
    [Flags]
    public enum NJTransformFlags
    {
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NJTransformAttribute : Attribute
    {
        internal readonly NJTransformFlags flags;

        // -------------------------------------------------------------------------------------------------------------

        public NJTransformAttribute(NJTransformFlags flags = NJTransformFlags.Position | NJTransformFlags.Rotation)
        {
            this.flags = flags;
        }
    }
}

partial class Util
{
    public static void WriteNJTransforms(this JObject jobj, object target, in Transform root)
    {
        foreach (var field in target.GetType().GetFields(BindingFlagsALL))
            if (field.FieldType == typeof(Transform) && field.GetValue(target) is Transform transform)
            {
                var attr = field.GetCustomAttribute<NJTransformAttribute>();
                if (attr == null)
                    continue;

                var jtfm = new JObject()
                {
                    ["relativePath"] = transform.GetRelativePath(root),
                };

                if (attr.flags.HasFlag(NJTransformFlags.Position))
                    jobj[nameof(transform.localPosition)] = JsonConvert.SerializeObject(transform.localPosition);

                if (attr.flags.HasFlag(NJTransformFlags.Rotation))
                    jobj[nameof(transform.localEulerAngles)] = JsonConvert.SerializeObject(transform.localEulerAngles.SignedEulers());

                if (attr.flags.HasFlag(NJTransformFlags.Scale))
                    jobj[nameof(transform.localScale)] = JsonConvert.SerializeObject(transform.localScale);

                jobj[field.Name] = jtfm;
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

                        if (attr.flags.HasFlag(NJTransformFlags.Position))
                            if (jtfm.TryGetValue(nameof(transform.localPosition), out var jpos))
                                transform.localPosition = jpos.ToObject<Vector3>();

                        if (attr.flags.HasFlag(NJTransformFlags.Rotation))
                            if (jtfm.TryGetValue(nameof(transform.localEulerAngles), out var jeul))
                                transform.localEulerAngles = jeul.ToObject<Vector3>();

                        if (attr.flags.HasFlag(NJTransformFlags.Scale))
                            if (jtfm.TryGetValue(nameof(transform.localScale), out var jscale))
                                transform.localScale = jscale.ToObject<Vector3>();

                        field.SetValue(target, transform);
                    }
            }
    }
}