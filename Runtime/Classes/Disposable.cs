using System;
using UnityEngine;

namespace _UTIL_
{
    public class Disposable : IDisposable
    {
        public readonly string name;
        public Action onDispose;
        public bool _disposed;

        static ushort _id;
        public readonly ushort disposable_id = _id++;

        public static readonly HashSetListener<Disposable> all_disposables = new();

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _id = 0;
            all_disposables.Reset();
        }

        //----------------------------------------------------------------------------------------------------------

        public Disposable(in string name)
        {
            this.name = name;
            all_disposables.AddElement(this);
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

            all_disposables.RemoveElement(this);

            OnDispose();

            onDispose?.Invoke();
            onDispose = null;
        }

        protected virtual void OnDispose()
        {
        }
    }
}