using System;
using System.Linq;
using UnityEngine;

namespace _UTIL_
{
    public class AnimCommand : IDisposable
    {
        public readonly AnimationEvent e;
        public bool _disposed;

        public int _start_i, _read_i;

        //----------------------------------------------------------------------------------------------------------

        public AnimCommand(in AnimationEvent e)
        {
            this.e = e;
        }

        //----------------------------------------------------------------------------------------------------------

        public bool HasNext()
        {
            while (_read_i < e.stringParameter.Length)
            {
                char c = e.stringParameter[++_read_i];
                if (c != ' ')
                    return true;
            }
            return false;
        }

        public bool TryRead(out string value)
        {
            if (!HasNext())
            {
                value = null;
                return false;
            }

            _start_i = _read_i;
            while (_read_i < e.stringParameter.Length)
            {
                char c = e.stringParameter[_read_i++];
                if (c == ' ')
                    break;
            }

            value = e.stringParameter[_start_i.._read_i];
            return true;
        }

        public bool TryRead(out string value, in bool ignore_case, params string[] expected)
        {
            int old_start = _start_i;
            int old_read = _read_i;

            if (TryRead(out value))
                if (expected.Contains(value, ignore_case ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal))
                    return true;

            _start_i = old_start;
            _read_i = old_read;

            return false;
        }

        public bool TryReadAll(out string value)
        {
            if (!HasNext())
            {
                value = null;
                return false;
            }
            _start_i = _read_i;
            value = e.stringParameter[_read_i..].Trim();
            _read_i += e.stringParameter.Length;
            return true;
        }

        public bool TryPeek(out string value)
        {
            int old_start = _start_i;
            int old_read = _read_i;

            bool yes = TryRead(out value);
            _start_i = old_start;
            _read_i = old_read;

            return yes;
        }

        public bool TryPeek(out char value)
        {
            if (_read_i < e.stringParameter.Length)
            {
                value = e.stringParameter[_read_i];
                return true;
            }
            value = default;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }
    }
}