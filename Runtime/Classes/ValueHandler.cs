using _UTIL_;
using System;

partial class Util
{
    public static void Toggle(this ValueHandler<bool> onValue) => onValue.Value = !onValue.Value;
    public static void Toggle(this ValueHandler<bool> onValue, bool value) => onValue.Value = value;
    public static void Toggle_inv(this ValueHandler<bool> onValue, bool value) => onValue.Value = !value;
    public static void AddListener(this ValueHandler<bool> onValue, Action onTrue, Action onFalse) => onValue.AddListener(value =>
    {
        if (value)
            onTrue?.Invoke();
        else
            onFalse?.Invoke();
    });
}

namespace _UTIL_
{
    public class ValueHandler : IDisposable
    {
        public readonly Type type;
        public bool _disposed;

        //------------------------------------------------------------------------------------------------------------------------------

        protected ValueHandler(in Type type)
        {
            this.type = type;
        }

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
    public class ValueHandler<T> : ValueHandler
    {
        public bool changed;
        public T _value, old;
        Action onChange;
        Action<T> onChangeT;
        public Func<T, T> processor;

        //------------------------------------------------------------------------------------------------------------------------------

        public ValueHandler(in T init = default) : base(typeof(T))
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
            onChangeT = null;
            processor = null;
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

        public void AddListener(in Action action, in bool doNotCallThisTime = false)
        {
            lock (this)
            {
                onChange -= action;
                onChange += action;
                if (!doNotCallThisTime)
                    action();
            }
        }

        public void AddListener(in Action<T> action, in bool doNotCallThisTime = false)
        {
            lock (this)
            {
                onChangeT -= action;
                onChangeT += action;
                if (!doNotCallThisTime)
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
                    onChangeT?.Invoke(value);
                    onChange?.Invoke();
                }

                return changed;
            }
        }
    }
}