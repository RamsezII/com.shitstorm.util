using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public abstract class CollectionListener<TCollection, TItem> where TCollection : ICollection<TItem>, new()
    {
        public readonly TCollection _collection;

        public Action<bool>
            _listeners1;

        public Action<TCollection>
            _listeners2,
            _listeners2_once;

        //------------------------------------------------------------------------------------------------------------------------------

        public CollectionListener()
        {
            _collection = new();
        }

        //------------------------------------------------------------------------------------------------------------------------------

        public TCollection Collection
        {
            get
            {
                lock (this)
                    return _collection;
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (this)
                    return _collection.Count == 0;
            }
        }

        public bool IsNotEmpty
        {
            get
            {
                lock (this)
                    return _collection.Count > 0;
            }
        }

        public void AddListener1b(Action<bool> action) => AddListener1(action);
        public void AddListener1(in Action<bool> action, in bool doNotCallThisTime = false)
        {
            lock (this)
            {
                _listeners1 -= action;
                _listeners1 += action;
                if (!doNotCallThisTime)
                    action(IsNotEmpty);
            }
        }

        public void AddListener2(in Action<TCollection> action, in bool doNotCallThisTime = false)
        {
            lock (this)
            {
                _listeners2 -= action;
                _listeners2 += action;
                if (!doNotCallThisTime)
                    action(_collection);
            }
        }

        public void Modify(in Action<TCollection> onCollection)
        {
            lock (this)
            {
                int count1 = _collection.Count;
                onCollection?.Invoke(_collection);
                int count2 = _collection.Count;

                _listeners2?.Invoke(_collection);

                if (_listeners1 != null)
                    if (count1 == 0 || count2 == 0)
                        if (count1 != count2)
                            _listeners1(IsNotEmpty);

                _listeners2_once?.Invoke(_collection);
                _listeners2_once = null;
            }
        }

        protected abstract void OnClear();
        public void Clear() => Modify(collection => OnClear());

        //------------------------------------------------------------------------------------------------------------------------------

        public void Reset()
        {
            lock (this)
            {
                _listeners1 = null;
                _listeners2 = null;
                _listeners2_once = null;
                OnClear();
            }
        }
    }
}