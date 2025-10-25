using System.Collections.Generic;

namespace _UTIL_
{
    public sealed class ListListener : ListListener<object>
    {
    }

    public class ListListener<T> : CollectionListener<List<T>, T>
    {
        public bool IsLast(in T element)
        {
            RemoveZombies();
            lock (this)
                return element != null && _collection.Count > 0 && _collection[^1].Equals(element);
        }

        public bool IsEmptyOrLast(in T element)
        {
            RemoveZombies();
            lock (this)
                return IsEmpty || element != null && IsLast(element);
        }

        protected override void OnRemoveZombies()
        {
            for (int i = 0; i < _collection.Count; i++)
                if (_collection[i] == null || _collection[i] is UnityEngine.Object ue && ue == null)
                    Modify(list => list.RemoveAt(i--));
        }

        public bool ToggleElement(T element, in bool toggle)
        {
            lock (this)
            {
                bool contained = _collection.Contains(element);
                if (contained)
                {
                    if (!toggle)
                        Modify(list => list.Remove(element));
                }
                else
                {
                    if (toggle)
                        Modify(list => list.Add(element));
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
                    Modify(list => list.Remove(element));
                    return false;
                }
                Modify(list => list.Add(element));
                return true;
            }
        }

        public void AddElement(T element)
        {
            lock (this)
            {
                bool empty = IsEmpty;
                if (IsNotEmpty)
                    if (element.Equals(_collection[^1]))
                        return;
                    else
                        _collection.Remove(element);
                Modify(list => list.Add(element));
            }
        }

        public bool RemoveElement(T element)
        {
            lock (this)
            {
                if (_collection.Contains(element))
                {
                    Modify(list => list.Remove(element));
                    return true;
                }
                return false;
            }
        }

        public void InsertElementAt(int index, T element)
        {
            lock (this)
                Modify(list => list.Insert(index, element));
        }

        public void RemoveElementAt(int index)
        {
            lock (this)
                Modify(list => list.RemoveAt(index));
        }

        public override void _Clear()
        {
            _collection.Clear();
        }
    }
}