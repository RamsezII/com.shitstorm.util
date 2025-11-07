using System;

namespace _UTIL_
{
    public sealed class Readyhandler
    {
        public bool is_ready;
        Action on_ready;

        //--------------------------------------------------------------------------------------------------------------

        public void RegisterOnReady(in Action action)
        {
            if (is_ready)
                action?.Invoke();
            else
                on_ready += action;
        }

        //--------------------------------------------------------------------------------------------------------------

        public void Reset()
        {
            is_ready = false;
            on_ready = null;
        }

        //--------------------------------------------------------------------------------------------------------------

        public void TriggerReady()
        {
            is_ready = true;
            on_ready?.Invoke();
            on_ready = null;
        }
    }
}