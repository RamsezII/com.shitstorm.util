using UnityEngine;

namespace _UTIL_
{
    public static class Util_smooths
    {
        public static bool NO_SMOOTH;
    }

    public abstract class Smooth<T> : OnValue<T> where T : struct
    {
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

        public bool SmoothDamp(in float damp, in float deltaTime) => Update(Util_smooths.NO_SMOOTH ? target : Mathf.SmoothDamp(_value, target, ref velocity, damp, Mathf.Infinity, deltaTime));

        public bool SmoothDamp(in float up, in float down, in float deltaTime) => Update(Util_smooths.NO_SMOOTH ? target : Mathf.SmoothDamp(_value, target, ref velocity, IsTargetHigher ? up : down, Mathf.Infinity, deltaTime));

        public bool SmoothDamp(in float up, in float down, in float limit, in float deltaTime) => Update(Util_smooths.NO_SMOOTH ? target : Mathf.SmoothDamp(_value, target, ref velocity, IsTargetHigher ? up : down, limit, deltaTime));

        public bool SmoothDampAngle(in float damp, in float deltaTime, in float maxSpeed = Mathf.Infinity) => Update(Util_smooths.NO_SMOOTH ? target : Mathf.SmoothDampAngle(_value, target, ref velocity, damp, maxSpeed, deltaTime));
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
            Vector2 val = Util_smooths.NO_SMOOTH ? target : Vector2.SmoothDamp(_value, target, ref velocity, damp, maxSpeed, deltaTime);
            sqr = val.sqrMagnitude;
            return Update(val);
        }

        public bool SmoothDamp(in float spring, in float damp, in float deltaTime, in float maxSpeed = Mathf.Infinity)
        {
            Vector2 val = Util_smooths.NO_SMOOTH ? target : Vector2.SmoothDamp(_value, target * spring - _value * (spring - 1), ref velocity, damp, maxSpeed, deltaTime);
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
            else if (Util_smooths.NO_SMOOTH)
                val = target;
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
            else if (Util_smooths.NO_SMOOTH)
                val = target;
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
            Vector3 val = Util_smooths.NO_SMOOTH ? target : Vector3.SmoothDamp(_value, target * spring - _value * (spring - 1), ref velocity, damp, maxSpeed, deltaTime);
            sqr = val.sqrMagnitude;
            return Update(val);
        }
    }
}