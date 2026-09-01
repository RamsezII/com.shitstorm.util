using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public class GroupNotifier<T> : GroupNotifier<T, object> where T : ICollection<object>, new()
    {
        public GroupNotifier(in CollectionListener<T, object> group) : base(group)
        {
        }
    }

    public class GroupNotifier<CollectionType, ItemType> : ValueNotifier<bool> where CollectionType : ICollection<ItemType>, new()
    {
        public CollectionListener<CollectionType, ItemType> _group;

        public GroupNotifier(in CollectionListener<CollectionType, ItemType> group)
        {
            SetGroup(group);
        }

        public void SetGroup(in CollectionListener<CollectionType, ItemType> group, in bool doNotCallThisTime = false)
        {
            if (_group != null)
                _group._listeners1 -= PropagateValue;
            _group = group;
            _group.AddListener1(PropagateValue, doNotCallThisTime: doNotCallThisTime);
        }

        void PropagateValue(bool value)
        {
            Value = value;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            SetGroup(null);
        }
    }

    public class HashSetListener : HashSetListener<object>
    {
    }

    public class HashSetListener<TItem> : CollectionListener<HashSet<TItem>, TItem>
    {
        public Action<TItem, bool>
            _listeners3;

        //------------------------------------------------------------------------------------------------------------------------------

        public void AddElement(TItem element)
        {
            lock (this)
                if (!_collection.Contains(element))
                {
                    Modify(set => set.Add(element));
                    _listeners3?.Invoke(element, true);
                }
        }

        public bool RemoveElement(TItem element)
        {
            lock (this)
            {
                bool contained = false;
                Modify(set => contained = set.Remove(element));
                _listeners3?.Invoke(element, false);
                return contained;
            }
        }


        public void AddListener3(in Action<TItem, bool> action, in bool do_not_call_this_time = false)
        {
            lock (this)
            {
                _listeners3 -= action;
                _listeners3 += action;

                if (!do_not_call_this_time)
                    foreach (var e in _collection)
                        action(e, true);
            }
        }

        public bool ToggleElement(TItem element)
        {
            lock (this)
                return ToggleElement(element, !_collection.Contains(element));
        }

        public bool ToggleElement(TItem element, in bool toggle)
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