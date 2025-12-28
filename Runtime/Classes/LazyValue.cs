using System;
using UnityEngine;

namespace _UTIL_
{
    public sealed class LazyValue<T>
    {
        readonly Func<T> _factory;
        public bool _ready;

#if UNITY_EDITOR
        public
#endif
        T _value;

        //----------------------------------------------------------------------------------------------------------

        public LazyValue(in Func<T> factory)
        {
            _factory = factory;
            Reset();
        }

        //----------------------------------------------------------------------------------------------------------

        public T Value
        {
            get
            {
                lock (this)
                {
                    if (!_ready)
                        return ForcedValue();
                    return _value;
                }
            }
            set
            {
                lock (this)
                    _value = value;
            }
        }

        public T ForcedValue()
        {
            lock (this)
            {
                Debug.Log($"loading {GetType()}".ToSubLog());
                _value = _factory();
                _ready = true;
            }
            return _value;
        }

        public void Reset()
        {
            lock (this)
            {
                _value = default;
                _ready = false;
            }
        }
    }
}