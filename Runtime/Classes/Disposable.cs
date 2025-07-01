using System;
using UnityEngine;

namespace _UTIL_
{
    public class Disposable : IDisposable
    {
        public Action onDispose;
        public bool _disposed;

        static ushort _id;
        public readonly ushort id = _id++;

#if UNITY_EDITOR
        string _ToString => ToString();
#endif

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            _id = 0;
        }

        //----------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            lock (this)
                return $"{GetType()}[{id}]";
        }

        public bool Disposed
        {
            get
            {
                lock (this)
                    return _disposed;
            }
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            lock (this)
            {
                if (_disposed)
                    return;
                _disposed = true;
            }

            OnDispose();

            onDispose?.Invoke();
            onDispose = null;
        }

        protected virtual void OnDispose()
        {
        }
    }
}