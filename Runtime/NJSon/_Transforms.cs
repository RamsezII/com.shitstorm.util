using Newtonsoft.Json.Linq;
using UnityEngine;

partial class Util
{
    public static JObject ToJObject(this Transform transform, in Transform root) => transform == null ? null : new()
    {
        ["path"] = root == null ? transform.GetPath(true) : transform.GetRelativePath(root),
        ["position"] = transform.localPosition.ToJObject(),
        ["rotation"] = transform.localEulerAngles.ToJObject(),
        ["scale"] = transform.localScale.ToJObject(),
    };

    public static Transform ToTransform(this JObject jobj, in Transform root)
    {
        string path = (string)jobj["path"];
        Transform T = root.ForceFind(path, force_new: false);
        T.localPosition = jobj["position"].ToVector3();
        T.localEulerAngles = jobj["rotation"].ToVector3();
        T.localScale = jobj["scale"].ToVector3();
        return T;
    }
}