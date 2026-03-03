using Newtonsoft.Json.Linq;
using UnityEngine;

partial class Util_njson
{
    public static JObject ToJObject(this Vector3 vector) => new()
    {
        ["x"] = vector.x,
        ["y"] = vector.y,
        ["z"] = vector.z,
    };

    public static Vector3 ToVector3(this JToken njson) => new(
        (float)(njson["x"] ?? 0f),
        (float)(njson["y"] ?? 0f),
        (float)(njson["z"] ?? 0f)
    );
}