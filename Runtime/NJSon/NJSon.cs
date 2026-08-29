using System;
using System.Reflection;
using _UTIL_;
using Newtonsoft.Json.Linq;

namespace _UTIL_
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NJFieldAttribute : Attribute
    {
    }
}

partial class Util
{
    public const BindingFlags BindingFlagsALL =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static;

    //----------------------------------------------------------------------------------------------------------

    public static void WriteNJFields(this JObject jobj, in object target, in Type type = null) => WriteFields<NJFieldAttribute>(jobj, target, type);
    public static void WriteFields<T>(this JObject jobj, in object target, in Type type = null) where T : Attribute
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

    public static void ReadNJFields(this JObject jobj, in object target, in Type type = null) => ReadFields<NJFieldAttribute>(jobj, target, type);
    public static void ReadFields<T>(this JObject jobj, in object target, in Type type = null) where T : Attribute
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
}