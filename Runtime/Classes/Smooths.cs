using System;
using UnityEngine;

namespace _UTIL_
{
    public abstract class Smooth<T> : OnValue<T> where T : struct
    {
        [Serializable]
        public struct Damp
        {
            public float up, down;
            public Damp(in float up, in float down)
            {
                this.up = up;
                this.down = down;
            }
            public readonly float Get(in bool isUp) => isUp ? up : down;
            public static implicit operator Damp(in float v) => new(v, v);
            public static implicit operator Vector2(in Damp d) => new(d.up, d.down);
            public static implicit operator Damp(in Vector2 v) => new(v.x, v.y);
        }

        public T velocity;
        public T target, delta;

        //----------------------------------------------------------------------------------------------------------

        public Smooth(in T init = default) : base(init) { target = init; }

        //----------------------------------------------------------------------------------------------------------

        public virtual void ResetValue(in T value = default)
        {
            Update(value, true);
            target = velocity = delta = value;
        }
    }

    [System.Serializable]
    public class SmoothFloat : Smooth<float>
    {
        private bool IsTargetHigher => Mathf.Abs(target) > Mathf.Abs(_value);

        //----------------------------------------------------------------------------------------------------------

        public SmoothFloat(in float init = default) : base(init) { }

        //----------------------------------------------------------------------------------------------------------

        public override bool Update(float value, in bool force = false)
        {
            if (force)
                delta = default;
            else
                delta = value - _value;
            return base.Update(value, true);
        }

        public bool SmoothDamp(in float damp, in float deltaTime) => Update(Mathf.SmoothDamp(_value, target, ref velocity, damp, Mathf.Infinity, deltaTime));

        [Obsolete]
        public bool SmoothDamp(in float up, in float down, in float deltaTime) => SmoothDamp(new Damp(up, down), deltaTime);
        public bool SmoothDamp(in Damp damp, in float deltaTime) => Update(Mathf.SmoothDamp(_value, target, ref velocity, damp.Get(IsTargetHigher), Mathf.Infinity, deltaTime));

        [Obsolete]
        public bool SmoothDamp(in float up, in float down, in float limit, in float deltaTime) => SmoothDamp(new Damp(up, down), limit, deltaTime);
        public bool SmoothDamp(in Damp damp, in float limit, in float deltaTime) => Update(Mathf.SmoothDamp(_value, target, ref velocity, damp.Get(IsTargetHigher), limit, deltaTime));

        public bool SmoothDampAngle(in float damp, in float deltaTime, in float maxSpeed = Mathf.Infinity) => Update(Mathf.SmoothDampAngle(_value, target, ref velocity, damp, maxSpeed, deltaTime));
    }

    public abstract class SmoothVector<T> : Smooth<T> where T : struct
    {
        [Min(0)] public float sqr;

        //----------------------------------------------------------------------------------------------------------

        public SmoothVector(in T init = default) : base(init) { }
    }

    [System.Serializable]
    public class SmoothVector2 : SmoothVector<Vector2>
    {
        public SmoothVector2(in Vector2 init = default) : base(init) { sqr = init.sqrMagnitude; }

        //----------------------------------------------------------------------------------------------------------

        public override bool Update(Vector2 value, in bool force = false)
        {
            if (force)
                delta = default;
            else
                delta = value - _value;
            return base.Update(value);
        }

        public bool SmoothDamp(in float damp, in float deltaTime, in float maxSpeed = Mathf.Infinity)
        {
            Vector2 val = Vector2.SmoothDamp(_value, target, ref velocity, damp, maxSpeed, deltaTime);
            sqr = val.sqrMagnitude;
            return Update(val);
        }

        public bool SmoothDamp(in float spring, in float damp, in float deltaTime, in float maxSpeed = Mathf.Infinity)
        {
            Vector2 val = Vector2.SmoothDamp(_value, target * spring - _value * (spring - 1), ref velocity, damp, maxSpeed, deltaTime);
            sqr = val.sqrMagnitude;
            return Update(val);
        }
    }

    [System.Serializable]
    public class SmoothVector3 : SmoothVector<Vector3>
    {
        public SmoothVector3(in Vector3 init = default) : base(init) { sqr = init.sqrMagnitude; }

        //----------------------------------------------------------------------------------------------------------

        public override bool Update(Vector3 value, in bool force = false)
        {
            if (force)
                delta = default;
            else
                delta = value - _value;
            return base.Update(value);
        }

        public bool SmoothDamp(in float damp, in float deltaTime, in float maxSpeed = Mathf.Infinity)
        {
            Vector3 val;
            if (damp < .01f)
            {
                val = target;
                velocity = Vector3.zero;
            }
            else
                val = Vector3.SmoothDamp(_value, target, ref velocity, damp, maxSpeed, deltaTime);
            sqr = val.sqrMagnitude;
            return Update(val);
        }

        public bool SmoothDampAngle(in float damp, in float deltaTime, in float maxSpeed = Mathf.Infinity)
        {
            Vector3 val;
            if (damp < .01f)
            {
                val = target;
                velocity = Vector3.zero;
            }
            else
            {
                val.x = Mathf.SmoothDampAngle(_value.x, target.x, ref velocity.x, damp, maxSpeed, deltaTime);
                val.y = Mathf.SmoothDampAngle(_value.y, target.y, ref velocity.y, damp, maxSpeed, deltaTime);
                val.z = Mathf.SmoothDampAngle(_value.z, target.z, ref velocity.z, damp, maxSpeed, deltaTime);
            }
            sqr = val.sqrMagnitude;
            return Update(val);
        }

        public bool SmoothDamp(in float spring, in float damp, in float deltaTime, in float maxSpeed = Mathf.Infinity)
        {
            Vector3 val = Vector3.SmoothDamp(_value, target * spring - _value * (spring - 1), ref velocity, damp, maxSpeed, deltaTime);
            sqr = val.sqrMagnitude;
            return Update(val);
        }
    }
}