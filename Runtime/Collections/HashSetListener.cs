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

        protected override void OnClear()
        {
            _collection.Clear();
        }
    }
}