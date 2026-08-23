using Newtonsoft.Json.Linq;

partial class Util
{
    public static void TryRead_out<T>(this JObject jobj, string name, out T value)
    {
        value = default;
        JToken token = jobj[name];
        if (token != null)
            value = token.ToObject<T>();
    }

    public static void TryRead_ref<T>(this JObject jobj, string name, ref T value)
    {
        JToken token = jobj[name];
        if (token != null)
            value = token.ToObject<T>();
    }
}