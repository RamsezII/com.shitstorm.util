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

        public T SafeValue
        {
            get
            {
                lock (this)
                    return _value;
            }
        }

        public T GetValue(in bool force_reload = false)
        {
            lock (this)
                if (force_reload || !_ready)
                    Load();
            return _value;
        }

        void Load()
        {
            lock (this)
            {
                _value = _factory();
                _ready = true;
            }
        }

        public void Reset() => Reset(_factory);
        public void Reset(in Func<T> factory)
        {
            lock (this)
            {
                _factory = factory;
                _value = default;
                _ready = false;
            }
        }
    }
}