using System;
using UnityEngine;

namespace _UTIL_
{
    public abstract class Interpolator2<T> where T : struct
    {
        public T a, b;
        public float lerp;
        public readonly float speed;
        readonly Func<T, T, float, T> lerpFunc;
        T GetLerp() => lerp >= 1 ? b : lerpFunc(a, b, lerp);

        //----------------------------------------------------------------------------------------------------------

        public Interpolator2(in float speed, in Func<T, T, float, T> lerpFunc, in T init = default)
        {
            this.speed = speed;
            this.lerpFunc = lerpFunc;
            a = b = init;
        }

        //----------------------------------------------------------------------------------------------------------

        public void Reset(in T value)
        {
            a = b = value;
            lerp = 1;
        }

        public void OnNewValue(in T value)
        {
            a = GetLerp();
            b = value;
            lerp = 0;
        }

        public T Update(in float deltaTime)
        {
            lerp += speed * deltaTime;
            return GetLerp();
        }
    }

    public class Interpolator2_F : Interpolator2<float>
    {
        public Interpolator2_F(in float speed, in float init = 0f) : base(speed, Mathf.Lerp, init)
        {
        }
    }
}