using _UTIL_;
using System;

partial class Util
{
    public static void Toggle(this OnValue<bool> self, bool value) => self.Value = value;
    public static void Toggle(this OnValue<bool> self) => self.Value = !self.Value;
}

namespace _UTIL_
{
    [Serializable]
    public class OnValue : OnValue<object>
    {
    }

    [Serializable]
    public class OnValue<T>
    {
        public bool changed;
        public T _value, old;
        public Action<T> onChange;
        public EventPropagator<T> _propagator;
        public Func<T, T> processor;
        [Obsolete] public Action<T> onUpdate;

        //------------------------------------------------------------------------------------------------------------------------------

        public OnValue(in T init = default)
        {
            _value = old = init;
        }

        //------------------------------------------------------------------------------------------------------------------------------

        public void Reset(in T value = default)
        {
            changed = false;
            _value = default;
            old = default;
            onChange = null;
            _propagator?.ClearListeners();
            processor = null;
            onUpdate = null;
            Update(value, true);
        }

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

        public void AddListener(in Action<T> action)
        {
            lock (this)
            {
                onChange -= action;
                onChange += action;
                action(Value);
            }
        }

        public void AddListener(in object user, in Action<T> action)
        {
            lock (this)
                (_propagator ??= new()).AddListener(Value, user, action);
        }

        public void RemoveListener(in Action<T> action)
        {
            lock (this)
            {
                onChange -= action;
                _propagator?.RemoveListener_action(action);
            }
        }

        public void AddProcessor(in Func<T, T> processor)
        {
            lock (this)
            {
                this.processor += processor;
                Value = _value;
            }
        }

        public virtual bool Update(T value, in bool force)
        {
            lock (this)
            {
                old = _value;

                if (processor != null)
                    value = processor(value);

                changed = !Util.Equals2(value, _value);
                _value = value;

                onUpdate?.Invoke(value);
                if (force || changed)
                {
                    onChange?.Invoke(value);
                    _propagator?.NotifyListeners(value);
                }

                return changed;
            }
        }
    }
}