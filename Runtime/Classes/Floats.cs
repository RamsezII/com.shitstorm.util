using System;
using UnityEngine;

namespace _UTIL_
{
    [Serializable]
    public struct Float2
    {
        public float a, b;

        //----------------------------------------------------------------------------------------------------------

        public Float2(in float a, in float b)
        {
            this.a = a;
            this.b = b;
        }

        //----------------------------------------------------------------------------------------------------------

        public static implicit operator Vector2(in Float2 f) => new(f.a, f.b);

        public static implicit operator Float2(in Vector2 v) => new(v.x, v.y);
    }

    [Serializable]
    public struct Float3
    {
        public float a, b, c;

        //----------------------------------------------------------------------------------------------------------

        public Float3(in float a, in float b, in float c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        //----------------------------------------------------------------------------------------------------------

        public static implicit operator Vector3(in Float3 f) => new(f.a, f.b, f.c);
        public static implicit operator Float3(in Vector3 v) => new(v.x, v.y, v.z);
    }

    [Serializable]
    public struct Float4
    {
        public float a, b, c, d;

        //----------------------------------------------------------------------------------------------------------

        public Float4(in float a, in float b, in float c, in float d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        //----------------------------------------------------------------------------------------------------------

        public static implicit operator Vector4(in Float4 f) => new(f.a, f.b, f.c, f.d);
        public static implicit operator Float4(in Vector4 v) => new(v.x, v.y, v.z, v.w);
    }
}