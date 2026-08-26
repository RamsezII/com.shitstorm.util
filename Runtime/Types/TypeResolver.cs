using System;
using System.Collections.Generic;
using System.Linq;

namespace _UTIL_
{
    public static class TypeResolver
    {
        static readonly Dictionary<Type, List<Type>> cache = new();

        //------------------------------------------------------------------------------------------------------------------------------

        public static List<Type> AllDerivedTypes(this Type type)
        {
            if (cache.TryGetValue(type, out var cached))
                return cached;

            var types = cache[type] = Util.EGetAllDerivedTypes(type).ToList();
            types.Sort((a, b) => a.FullName.CompareTo(b.FullName));

            return types;
        }
    }
}