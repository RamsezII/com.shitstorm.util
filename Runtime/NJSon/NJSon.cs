using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace _UTIL_
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NJFieldAttribute : Attribute
    {
        public readonly bool editable;

        //--------------------------------------------------------------------------------------------------------------

        public NJFieldAttribute(bool editable = true)
        {
            this.editable = editable;
        }
    }
}

public sealed class NJDict : Dictionary<Type, JObject>
{
    public readonly Type limit;

    //--------------------------------------------------------------------------------------------------------------

    public NJDict(Type included_limit)
    {
        limit = included_limit?.BaseType ?? null;
    }

    //--------------------------------------------------------------------------------------------------------------

    public JObject GetOrAddLayerJObject<T>()
    {
        var type = typeof(T);

        if (!TryGetValue(type, out var jobj))
            Add(type, jobj = new());

        return jobj;
    }

    public void SaveTexts<T>(Func<Type, string> getPath, bool log, in object target, in Type type = null) where T : Attribute
    {
        foreach (var field in (type ?? target.GetType()).EFieldsByLayer(limit))
        {
            var attr = field.GetCustomAttribute<T>();
            if (attr == null)
                continue;

            if (!TryGetValue(field.DeclaringType, out var jobj))
                Add(field.DeclaringType, jobj = new());

            object value = field.GetValue(target);
            jobj[field.Name] = value == null ? JValue.CreateNull() : JToken.FromObject(value, Util.njSerializer);
        }

        if (Count > 0)
            foreach (var pair in this)
            {
                string spath = getPath(pair.Key);
                pair.Value.NJSave(spath, log);
            }
    }

    IEnumerable<Type> ETextLayers<TAttribute, TTextAttribute>(Type targetType) where TAttribute : Attribute where TTextAttribute : Attribute
    {
        for (var t = targetType; t != null && t != limit; t = t.BaseType)
            if (t.IsDefined(typeof(TTextAttribute), inherit: false) || t.GetFields(Util.BindingFlagsALL | BindingFlags.DeclaredOnly).Any(field => field.IsDefined(typeof(TAttribute), inherit: false)))
                yield return t;
    }

    public void LoadTexts<TAttribute, TTextAttribute>(Type targetType, Func<Type, string> getPath, bool log) where TAttribute : Attribute where TTextAttribute : Attribute
    {
        foreach (var t in ETextLayers<TAttribute, TTextAttribute>(targetType))
        {
            string path = getPath(t);
            if (path.TryNJRead(out JObject jobj, log_success: log, log_failure: t == targetType))
                Add(t, jobj);
        }
    }

    public void LoadRTexts<TAttribute, TTextAttribute>(Type targetType, bool log) where TAttribute : Attribute where TTextAttribute : Attribute
    {
        foreach (var t in ETextLayers<TAttribute, TTextAttribute>(targetType))
        {
            string rname = t.GetJSonFileName_noTXT();
            if (rname.TryNJRead_resource(out JObject jobj, log_success: log, log_failure: t == targetType))
                Add(t, jobj);
        }
    }

    public void SetFields<TAttribute>(object target, Type targetType = null) where TAttribute : Attribute
    {
        foreach (var field in (targetType ?? target.GetType()).EFieldsByLayer(limit))
        {
            var attr = field.GetCustomAttribute<TAttribute>();
            if (attr == null)
                continue;

            if (!TryGetValue(field.DeclaringType, out var jobj))
                continue;

            if (!jobj.TryGetValue(field.Name, out JToken token))
                continue;

            if (token.Type != JTokenType.Null)
                field.SetValue(target, token.ToObject(field.FieldType, Util.njSerializer));
            else
            {
                // null autorisé uniquement pour références et Nullable<T>
                if (!field.FieldType.IsValueType || Nullable.GetUnderlyingType(field.FieldType) != null)
                    field.SetValue(target, null);
            }
        }
    }
}

partial class Util
{
    public const BindingFlags BindingFlagsALL =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static;

    internal static readonly JsonSerializer njSerializer = CreateNJSerializer();

    static JsonSerializer CreateNJSerializer()
    {
        JsonSerializer serializer = new();
        serializer.Converters.Add(new UnityStructJsonConverter());
        return serializer;
    }

    //----------------------------------------------------------------------------------------------------------

    public static IEnumerable<FieldInfo> EFields<T>(this object target, in Type type = null) where T : Attribute => (type ?? target.GetType()).EFieldsByLayer().Where(field => field.GetCustomAttribute<T>() != null);
    public static IEnumerable<(FieldInfo field, T attribute)> EFieldsAndAttributes<T>(this object target, in Type type = null) where T : Attribute => (type ?? target.GetType()).EFieldsByLayer().Select(field => (field, field.GetCustomAttribute<T>())).Where(pair => pair.Item2 != null);

    public static IEnumerable<FieldInfo> EFieldsByLayer(this Type type, Type limit = null)
    {
        for (var layer = type; layer != null && layer != limit; layer = layer.BaseType)
            foreach (var field in layer.GetFields(BindingFlagsALL | BindingFlags.DeclaredOnly))
                yield return field;
    }

    public static void WriteStaticFields<T>(this JObject jobj, in Type type) where T : Attribute => jobj.WriteFields<T>(target: null, type: type);
    public static void WriteFields<T>(this JObject jobj, in object target, in Type type = null) where T : Attribute
    {
        foreach (var field in EFieldsByLayer(type ?? target.GetType()))
        {
            var attr = field.GetCustomAttribute<T>();
            if (attr == null)
                continue;

            object value = field.GetValue(target);
            jobj[field.Name] = value == null ? JValue.CreateNull() : JToken.FromObject(value, njSerializer);
        }
    }

    public static void ReadStaticFields<T>(this JObject jobj, in Type type) where T : Attribute => jobj.ReadFields<T>(target: null, type: type);
    public static void ReadFields<T>(this JObject jobj, in object target, in Type type = null) where T : Attribute
    {
        foreach (var field in EFieldsByLayer(type ?? target.GetType()))
        {
            var attr = field.GetCustomAttribute<T>();
            if (attr == null)
                continue;

            if (!jobj.TryGetValue(field.Name, out JToken token))
                continue;

            if (token.Type != JTokenType.Null)
                field.SetValue(target, token.ToObject(field.FieldType, njSerializer));
            else
            {
                // null autorisé uniquement pour références et Nullable<T>
                if (!field.FieldType.IsValueType || Nullable.GetUnderlyingType(field.FieldType) != null)
                    field.SetValue(target, null);
            }
        }
    }
}

sealed class UnityStructJsonConverter : JsonConverter
{
    static readonly HashSet<Type> supportedTypes = new()
    {
        typeof(Vector2),
        typeof(Vector3),
        typeof(Vector4),
        typeof(Vector2Int),
        typeof(Vector3Int),
        typeof(Quaternion),
        typeof(Color),
        typeof(Color32),
        typeof(Rect),
        typeof(RectInt),
        typeof(Bounds),
        typeof(BoundsInt),
        typeof(Ray),
        typeof(Ray2D),
        typeof(Plane),
        typeof(Matrix4x4),
        typeof(LayerMask),
    };

    public override bool CanConvert(Type objectType) => supportedTypes.Contains(objectType);

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JToken.Parse(JsonUtility.ToJson(value)).WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return Activator.CreateInstance(objectType);

        JToken token = JToken.Load(reader);
        return JsonUtility.FromJson(token.ToString(Formatting.None), objectType);
    }
}
