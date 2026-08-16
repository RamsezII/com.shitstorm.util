using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public static class TypeResolver
    {
        static readonly Dictionary<string, Type> cache = new();

        //------------------------------------------------------------------------------------------------------------------------------

        public static Type GetType(this string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            if (cache.TryGetValue(typeName, out var cached))
                return cached;

            // Fonctionne notamment avec un nom assembly-qualified.
            var type = Type.GetType(typeName);

            if (type != null)
            {
                cache[typeName] = type;
                return type;
            }

            // Recherche dans toutes les assemblies actuellement chargées.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);

                if (type != null)
                {
                    cache[typeName] = type;
                    return type;
                }
            }

            return null;
        }
    }
}