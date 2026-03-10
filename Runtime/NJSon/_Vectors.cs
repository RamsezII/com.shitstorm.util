using Newtonsoft.Json.Linq;
using UnityEngine;

partial class Util
{
    public static JObject ToJObject(this Vector3 vector) => new()
    {
        ["x"] = vector.x,
        ["y"] = vector.y,
        ["z"] = vector.z,
    };

    public static Vector3 ToVector3(this JToken njson, in string name) => ToVector3(njson.Value<JObject>(name));
    public static Vector3 ToVector3(this JToken njson) => new(
        (float)(njson["x"] ?? 0f),
        (float)(njson["y"] ?? 0f),
        (float)(njson["z"] ?? 0f)
    );
}