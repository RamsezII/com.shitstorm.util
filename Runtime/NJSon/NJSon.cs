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
        public NJFieldAttribute(bool editable = true)
        {
            this.editable = editable;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NJSliderAttribute : NJFieldAttribute
    {
        internal readonly float min, max;
        public NJSliderAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
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

    static readonly JsonSerializer njSerializer = CreateNJSerializer();

    static JsonSerializer CreateNJSerializer()
    {
        JsonSerializer serializer = new();
        serializer.Converters.Add(new UnityStructJsonConverter());
        return serializer;
    }

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
            jobj[field.Name] = value == null ? JValue.CreateNull() : JToken.FromObject(value, njSerializer);
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
