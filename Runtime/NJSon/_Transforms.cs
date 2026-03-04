using Newtonsoft.Json.Linq;
using UnityEngine;

partial class Util_njson
{
    public static JObject ToJObject(this Transform transform, in Transform root) => transform == null ? null : new()
    {
        ["path"] = root == null ? transform.GetPath(true) : transform.GetRelativePath(root),
        ["position"] = transform.localPosition.ToJObject(),
        ["rotation"] = transform.localEulerAngles.ToJObject(),
        ["scale"] = transform.localScale.ToJObject(),
    };

    public static Transform ToTransform(this JObject njson, in Transform root)
    {
        string path = (string)njson["path"];
        Transform T = root.ForceFind(path, force_new: false);
        T.localPosition = njson["position"].ToVector3();
        T.localEulerAngles = njson["rotation"].ToVector3();
        T.localScale = njson["scale"].ToVector3();
        return T;
    }
}