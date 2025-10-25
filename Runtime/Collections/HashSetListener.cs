using System.Collections.Generic;

namespace _UTIL_
{
    public class HashSetListener : HashSetListener<object>
    {
    }

    public class HashSetListener<T> : CollectionListener<HashSet<T>, T>
    {
        public bool ToggleElement(T element, bool toggle)
        {
            lock (this)
            {
                bool contained = _collection.Contains(element);
                if (contained)
                {
                    if (!toggle)
                        Modify(set => set.Remove(element));
                }
                else
                {
                    if (toggle)
                        Modify(set => set.Add(element));
                }
                return contained;
            }
        }

        public bool ToggleElement(T element)
        {
            lock (this)
            {
                if (_collection.Contains(element))
                {
                    Modify(set => set.Remove(element));
                    return false;
                }
                Modify(set => set.Add(element));
                return true;
            }
        }

        public void AddElement(T element)
        {
            lock (this)
                if (!_collection.Contains(element))
                    Modify(set => set.Add(element));
        }

        public bool RemoveElement(T element)
        {
            lock (this)
            {
                if (_collection.Contains(element))
                {
                    Modify(set => set.Remove(element));
                    return true;
                }
                return false;
            }
        }

        public override void _Clear()
        {
            _collection.Clear();
        }

        protected override void OnRemoveZombies()
        {
            HashSet<T> copies = new();

            foreach (T item in _collection)
                if (item != null && (item is not UnityEngine.Object ue || ue != null))
                    copies.Add(item);

            _collection.Clear();

            foreach (T item in copies)
                _collection.Add(item);
        }
    }
}