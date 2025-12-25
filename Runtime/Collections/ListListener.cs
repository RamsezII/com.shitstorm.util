using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public sealed class ListListener : ListListener<object>
    {
    }

    public class ListListener<T> : CollectionListener<List<T>, T>
    {
        public T LastOrDefault => _collection.Count == 0 ? default : _collection[^1];
        public bool IsFirst(in T element) => IsAtIndex(element, 0);
        public bool IsLast(in T element) => IsAtIndex(element, ^1);
        public bool IsAtIndex(in T element, in Index index)
        {
            lock (this)
                return element != null && _collection.Count > 0 && _collection[index].Equals(element);
        }

        public bool TryPeek(out T element)
        {
            lock (this)
            {
                if (IsNotEmpty)
                {
                    element = _collection[^1];
                    return true;
                }
                element = default;
                return false;
            }
        }

        public IEnumerable<(int index, T element)> ReversedOrderIteration()
        {
            lock (this)
                if (_collection != null && _collection.Count > 0)
                    for (int i = _collection.Count - 1; i >= 0; i--)
                        yield return (i, _collection[i]);
        }

        public bool IsEmptyOrLast(in T element)
        {
            lock (this)
                return IsEmpty || IsAtIndex(element, ^1);
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

        protected override void OnClear()
        {
            _collection.Clear();
        }
    }
}