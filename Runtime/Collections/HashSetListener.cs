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

        public bool ToggleElement(T element)
        {
            lock (this)
                return ToggleElement(element, !_collection.Contains(element));
        }

        public bool ToggleElement(T element, in bool toggle)
        {
            lock (this)
            {
                bool contained = _collection.Contains(element);
                if (contained)
                {
                    if (!toggle)
                        RemoveElement(element);
                }
                else
                {
                    if (toggle)
                        AddElement(element);
                }
                return contained;
            }
        }

        protected override void OnClear()
        {
            _collection.Clear();
        }
    }
}