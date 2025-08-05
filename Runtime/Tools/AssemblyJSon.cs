#if UNITY_EDITOR
using System;

namespace _UTIL_e
{
    [Serializable]
    internal class AsmDef
    {
        public string name;
        public string[] references;
    }
}
#endif