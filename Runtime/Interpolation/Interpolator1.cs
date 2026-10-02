using System;
using UnityEngine;

namespace _UTIL_
{
    public abstract class Interpolator1<T> where T : struct
    {
        public T a, b;
        public float ta, tb;

        public T last;
        public float tlast;

        //----------------------------------------------------------------------------------------------------------

        public void OnValue(in T value, in float time)
        {
            a = last;
            ta = tlast;
            last = b = value;
            tlast = tb = time;
        }

        public T Interp(in float time)
        {
            tlast = time;

            float lerp = ta >= tb ? 1 : Mathf.InverseLerp(ta, tb, time);
            if (lerp >= 1)
                return last = b;

            return last = Lerp(lerp);
        }

        protected abstract T Lerp(in float lerp);

        protected abstract T SlerpAround(in float lerp, in Vector3 pivot);
    }

    [Serializable]
    public class Interpolator1_V3 : Interpolator1<Vector3>
    {
        protected override Vector3 Lerp(in float lerp) => Vector3.Lerp(a, b, lerp);
        protected override Vector3 SlerpAround(in float lerp, in Vector3 pivot) => pivot + Vector3.Slerp(a - pivot, b - pivot, lerp);
    }

    [Serializable]
    public class Interpolator1_Q : Interpolator1<Quaternion>
    {
        protected override Quaternion Lerp(in float lerp) => Quaternion.Slerp(a, b, lerp);
        protected override Quaternion SlerpAround(in float lerp, in Vector3 pivot) => throw new NotImplementedException();
    }

    [Serializable]
    public readonly struct RgbInfos1
    {
        public readonly Vector3 position;
        public readonly Vector3 velocity;

        //----------------------------------------------------------------------------------------------------------

        public RgbInfos1(in Vector3 position, in Vector3 velocity)
        {
            this.position = position;
            this.velocity = velocity;
        }
    }
}