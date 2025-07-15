using System;
using System.Collections;

namespace _UTIL_
{
    public abstract class CollectionListener<T> where T : ICollection, new()
    {
        public readonly T _collection;
        public readonly EventPropagator<bool> _listeners1 = new();
        public readonly EventPropagator<T> _listeners2 = new(), _listeners2_once = new();

        //------------------------------------------------------------------------------------------------------------------------------

        public CollectionListener(in T collection)
        {
            _collection = collection;
        }

        public CollectionListener() : this(new T())
        {
        }

        //------------------------------------------------------------------------------------------------------------------------------

        public bool IsEmpty
        {
            get
            {
                RemoveZombies();
                lock (this)
                    return _collection.Count == 0;
            }
        }

        public bool IsNotEmpty
        {
            get
            {
                RemoveZombies();
                lock (this)
                    return _collection.Count > 0;
            }
        }

        protected abstract void OnRemoveZombies();
        protected void RemoveZombies()
        {
            if (_collection.Count > 0)
                OnRemoveZombies();
        }

        public void AddListener1(in object user, in Action<bool> action)
        {
            RemoveZombies();
            lock (this)
                _listeners1.AddListener(IsNotEmpty, user, action);
        }

        public void AddListener2(in object user, in Action<T> action)
        {
            RemoveZombies();
            lock (this)
                _listeners2.AddListener(_collection, user, action);
        }

        public void AddListener2_once(in object user, in Action<T> action)
        {
            RemoveZombies();
            lock (this)
                _listeners2_once.AddListener(_collection, user, action);
        }

        public void Modify(in Action<T> onCollection)
        {
            lock (this)
            {
                int count1 = _collection.Count;
                onCollection(_collection);
                RemoveZombies();
                int count2 = _collection.Count;

                _listeners2.NotifyListeners(_collection);

                if (count1 != count2)
                    if (count1 == 0 || count2 == 0)
                        _listeners1.NotifyListeners(IsNotEmpty);

                _listeners2_once.NotifyListeners(_collection);
                _listeners2_once._listeners.Clear();
            }
        }

        public abstract void _Clear();
        public void Clear() => Modify(collection => _Clear());

        //------------------------------------------------------------------------------------------------------------------------------

        public void Reset()
        {
            lock (this)
            {
                _listeners1.Dispose();
                _listeners2.Dispose();
                _listeners2_once.Dispose();
                _Clear();
            }
        }
    }
}