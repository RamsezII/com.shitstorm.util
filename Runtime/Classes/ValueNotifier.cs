using _UTIL_;
using System;
using UnityEngine;

partial class Util
{
    public static void ToggleAuto(this ValueNotifier<bool> onValue) => onValue.Value = !onValue.Value;
    public static void ToggleValue(this ValueNotifier<bool> onValue, bool value) => onValue.Value = value;
    public static void Toggle_inv(this ValueNotifier<bool> onValue, bool value) => onValue.Value = !value;
    public static void AddListener(this ValueNotifier<bool> onValue, Action onTrue, Action onFalse) => onValue.AddListener(value =>
    {
        if (value)
            onTrue?.Invoke();
        else
            onFalse?.Invoke();
    });
}

namespace _UTIL_
{
    [Serializable]
    public class ValueNotifier : IDisposable
    {
        public bool _disposed;

        //------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            lock (this)
            {
                if (_disposed)
                    return;
                _disposed = true;
                OnDispose();
            }
        }

        protected virtual void OnDispose()
        {
        }
    }

    [Serializable]
    public class ValueNotifier<T> : ValueNotifier
    {
        public bool changed;
        public int last_frame;
        public T _value, old;
        Action onChange;
        public Action onChange_once;
        Action<T> onChangeT;
        public Action<T> onChangeT_once;
        public Func<T, T> processor;
        public bool Has => Value != null;
        public bool Had => old != null;

        //------------------------------------------------------------------------------------------------------------------------------

        public ValueNotifier(in T init = default)
        {
            _value = old = init;
        }

        //------------------------------------------------------------------------------------------------------------------------------

        public bool HasT(out T value)
        {
            value = _value;
            return Has;
        }

        public bool HadT(out T old)
        {
            old = this.old;
            return Had;
        }

        public bool WasLastChangedThisFrame => changed && last_frame == Time.frameCount;

        public void Reset()
        {
            last_frame = Time.frameCount;
            changed = false;
            _value = default;
            old = default;
            onChange = null;
            onChangeT = null;
            processor = null;
            OnReset();
        }

        protected virtual void OnReset()
        {
        }

        public void Revert() => Value = old;

        public T Value
        {
            get
            {
                lock (this)
                    return _value;
            }
            set => Update(value, false);
        }

        public bool TryPullValue(out T value)
        {
            lock (this)
            {
                value = _value;
                if (Util.Equals2(_value, default))
                    return false;
                _value = default;
                return true;
            }
        }

        public T PullValue()
        {
            lock (this)
            {
                T temp = _value;
                Value = default;
                return temp;
            }
        }

        public bool PullChanged()
        {
            lock (this)
                if (changed)
                {
                    changed = false;
                    return true;
                }
                else
                    return false;
        }

        public void AddListener(in Action action, in bool do_not_call_this_time = false)
        {
            lock (this)
            {
                onChange -= action;
                onChange += action;
                if (!do_not_call_this_time)
                    action();
            }
        }

        public void AddListener(in Action<T> action, in bool do_not_call_this_time = false)
        {
            lock (this)
            {
                onChangeT -= action;
                onChangeT += action;
                if (!do_not_call_this_time)
                    action(Value);
            }
        }

        public void RemoveListener(in Action action)
        {
            lock (this)
                onChange -= action;
        }

        public void RemoveListener(in Action<T> action)
        {
            lock (this)
                onChangeT -= action;
        }

        public void AddProcessor(in Func<T, T> processor)
        {
            lock (this)
            {
                this.processor += processor;
                Value = _value;
            }
        }

        public void Update(T value) => Update(value, false);
        public virtual bool Update(T value, in bool force)
        {
            lock (this)
            {
                old = _value;

                if (processor != null)
                    value = processor(value);

                changed = !Util.Equals2(value, _value);
                _value = value;

                if (force || changed)
                {
                    onChangeT_once?.Invoke(value);
                    onChangeT_once = null;

                    onChange_once?.Invoke();
                    onChange_once = null;

                    onChangeT?.Invoke(value);
                    onChange?.Invoke();
                }

                if (changed)
                    last_frame = Time.frameCount;

                return changed;
            }
        }
    }
}