using System;
using System.Linq;
using System.Reflection;

partial class Util
{
    public static object ModifyAnonymous<T>(this T target, string property, object value)
    {
        Type type = target.GetType();
        PropertyInfo[] props = type.GetProperties();
        object[] values = props.Select(p => p.Name.Equals(property, StringComparison.Ordinal) ? value : p.GetValue(target)).ToArray();

        return type.GetConstructors().First().Invoke(values);
    }
}