using System.Collections.Generic;
using System.Reflection;
using System;

partial class Util
{
    public static IEnumerable<Type> EGetAllDerivedTypes<T>(bool include_abstracts = false) where T : class
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Assembly assembly = assemblies[i];
            foreach (Type type in assembly.GetTypes())
                if (include_abstracts || !type.IsAbstract)
                    if (typeof(T).IsAssignableFrom(type))
                        yield return type;
        }
    }

    public static bool TryGetType(this string typeName, out Type type, in bool include_abstracts = false)
    {
        type = Type.GetType(typeName);
        if (type != null)
            if (include_abstracts || !type.IsAbstract)
                return true;

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null)
                if (include_abstracts || !type.IsAbstract)
                    return true;
        }

        type = null;
        return false;
    }

    public static bool TryGetType<T>(this string typeName, out Type type, in bool include_abstracts = false) where T : class
    {
        if (TryGetType(typeName, out type))
            if (include_abstracts || !type.IsAbstract)
                if (typeof(T).IsAssignableFrom(type))
                    return true;

        type = null;
        return false;
    }
}