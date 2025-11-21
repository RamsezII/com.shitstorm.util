using System;

namespace _UTIL_
{
    public sealed class LazyValue<T>
    {
        public Func<T> _factory;
        public T _value;
        public bool _ready;

        //----------------------------------------------------------------------------------------------------------

        public LazyValue(in Func<T> factory) => Reset(factory);

        //----------------------------------------------------------------------------------------------------------

        public T GetValue()
        {
            lock (this)
                if (!_ready)
                {
                    _value = _factory();
                    _ready = true;
                }
            return _value;
        }

        public void Reset() => Reset(_factory);
        public void Reset(in Func<T> factory)
        {
            this._factory = factory;
            _value = default;
            _ready = false;
        }
    }
}