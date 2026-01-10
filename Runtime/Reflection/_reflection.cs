using System.Collections.Generic;
using System.Reflection;
using System;

partial class Util
{
    public static IEnumerable<Type> EGetAllDerivedTypes<T>(bool include_abstracts = false) where T : class => EGetAllDerivedTypes(typeof(T), include_abstracts);
    public static IEnumerable<Type> EGetAllDerivedTypes(Type type, bool include_abstracts = false)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Assembly assembly = assemblies[i];
            foreach (Type candidate in assembly.GetTypes())
                if (include_abstracts || !candidate.IsAbstract)
                    if (type.IsAssignableFrom(candidate))
                        yield return candidate;
        }
    }

    public static IEnumerable<Type> EAllTypes()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type in assembly.GetTypes())
                yield return type;
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