using System.Collections.Generic;

namespace _UTIL_
{
    public class HashSetListener : HashSetListener<object>
    {
    }

    public class HashSetListener<T> : CollectionListener<HashSet<T>, T>
    {
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
                bool contained = false;
                Modify(set => contained = set.Remove(element));
                return contained;
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