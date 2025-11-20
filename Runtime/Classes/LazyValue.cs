using System;

namespace _UTIL_
{
    public sealed class LazyValue<T>
    {
        Func<T> factory;
        T value;
        bool ready;

        //----------------------------------------------------------------------------------------------------------

        public LazyValue(in Func<T> factory) => Reset(factory);

        //----------------------------------------------------------------------------------------------------------

        public T GetValue()
        {
            lock (this)
                if (!ready)
                {
                    value = factory();
                    ready = true;
                }
            return value;
        }

        public void Reset(in Func<T> factory)
        {
            this.factory = factory;
            value = default;
            ready = false;
        }
    }
}