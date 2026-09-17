using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

partial class Util
{
    static MemberInfo FindMember(Type type, string name)
    {
        for (var t = type; t != null; t = t.BaseType)
        {
            var field = t.GetField(
                name,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);

            if (field != null)
                return field;

            var property = t.GetProperty(
                name,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);

            if (property != null)
                return property;
        }

        return null;
    }

    static Type GetMemberType(MemberInfo member) => member switch
    {
        FieldInfo f => f.FieldType,
        PropertyInfo p => p.PropertyType,
        _ => throw new NotSupportedException()
    };

    static object GetMemberValue(MemberInfo member, object target) => member switch
    {
        FieldInfo f => f.GetValue(target),
        PropertyInfo p => p.GetValue(target),
        _ => throw new NotSupportedException()
    };

    static void SetMemberValue(MemberInfo member, object target, object value)
    {
        switch (member)
        {
            case FieldInfo f:
                f.SetValue(target, value);
                break;

            case PropertyInfo p:
                p.SetValue(target, value);
                break;

            default:
                throw new NotSupportedException();
        }
    }

    public static void WriteMember_custom<T>(this JObject jobj, string memberName, T value)
    {
        jobj[memberName] = value is null ? JValue.CreateNull() : JToken.FromObject(value, njSerializer);
    }

    public static void WriteMember(this JObject jobj, object target, string fieldName)
    {
        var member = FindMember(target.GetType(), fieldName) ?? throw new MissingMemberException(target.GetType().FullName, fieldName);

        object value = GetMemberValue(member, target);

        WriteMember_custom(jobj, fieldName, value);
    }

    public static bool ReadMember(this JObject jobj, object target, string fieldName, object defaultValue = null)
    {
        var member = FindMember(target.GetType(), fieldName) ?? throw new MissingMemberException(target.GetType().FullName, fieldName);

        object value = ReadMember_custom(jobj, fieldName, GetMemberType(member), defaultValue);
        SetMemberValue(member, target, value);
        return jobj.ContainsKey(fieldName);
    }

    public static T ReadMember_custom<T>(this JObject jobj, string memberName, T defaultValue = default)
    {
        return (T)ReadMember_custom(jobj, memberName, typeof(T), defaultValue);
    }

    static object ReadMember_custom(JObject jobj, string memberName, Type memberType, object defaultValue)
    {
        if (!jobj.TryGetValue(memberName, out var token) || token.Type is JTokenType.Null or JTokenType.Undefined)
            return defaultValue;

        try
        {
            return token.ToObject(memberType, njSerializer);
        }
        catch
        {
            return defaultValue;
        }
    }
}
