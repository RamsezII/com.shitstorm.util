using System;
using UnityEngine;

namespace _UTIL_
{
    [Serializable]
    public class Disposable : IDisposable
    {
        public readonly string name;
        public Action onDispose;
        public bool _disposed;

        static ushort _id;
        public readonly ushort disposable_id = _id++;

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _id = 0;
        }

        //----------------------------------------------------------------------------------------------------------

        public Disposable(in string name = null)
        {
            this.name = name ?? GetType().FullName;
        }

        //----------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            lock (this)
                return $"{{ {name} [{disposable_id}] ({GetType()}) }}";
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