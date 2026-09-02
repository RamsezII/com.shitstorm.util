using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace _UTIL_
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NJTextAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NJEditAttribute : NJTextAttribute
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

    public static IEnumerable<FieldInfo> EFields(this object target, in Type type = null) => (type ?? target.GetType()).GetFields(BindingFlagsALL);
    public static IEnumerable<FieldInfo> EFields<T>(this object target, in Type type = null) where T : Attribute => (type ?? target.GetType()).GetFields(BindingFlagsALL).Where(field => field.GetCustomAttribute<T>() != null);
    public static IEnumerable<(FieldInfo field, T attribute)> EFieldsAndAttributes<T>(this object target, in Type type = null) where T : Attribute => (type ?? target.GetType()).GetFields(BindingFlagsALL).Select(field => (field, field.GetCustomAttribute<T>())).Where(pair => pair.Item2 != null);

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