using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Scripting.LifecycleManagement;

namespace _UTIL_
{
    public static partial class TypeResolver
    {
        [AutoStaticsCleanup] static readonly Dictionary<Type, Type[]> cache = new();

        //------------------------------------------------------------------------------------------------------------------------------

        public static Type[] AllDerivedTypes(this Type type)
        {
            lock (cache)
            {
                if (cache.TryGetValue(type, out var cached))
                    return cached;

                var types = cache[type] = Util.EGetAllDerivedTypes(type).ToArray();
                Array.Sort(types, (a, b) => a.FullName.CompareTo(b.FullName));
                return types;
            }
        }
    }
}