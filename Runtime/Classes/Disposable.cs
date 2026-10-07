using System;
using Unity.Scripting.LifecycleManagement;

namespace _UTIL_
{
    [Serializable]
    public partial class Disposable : IDisposable
    {
        public readonly string name;
        public Action onDispose;
        public bool _disposed;

        [AutoStaticsCleanup] static ushort _id;
        public readonly ushort disposable_id = _id++;

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