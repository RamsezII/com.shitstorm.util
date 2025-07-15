using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public class DictListener<Key, Value> : CollectionListener<Dictionary<Key, Value>>
    {
        public Type
            key_type = typeof(Key),
            value_type = typeof(Value);

        //--------------------------------------------------------------------------------------------------------------

        protected override void OnRemoveZombies()
        {
            HashSet<Key> keys = null;

            foreach (var pair in _collection)
                if (pair.Value == null)
                    (keys ??= new()).Add(pair.Key);

            if (keys != null && keys.Count > 0)
                Modify(dict =>
                {
                    foreach (var key in keys)
                        dict.Remove(key);
                });
        }

        public void AddElement(Key key, Value value, bool force = false) => Modify(dict =>
        {
            if (force)
                _collection[key] = value;
            else
                _collection.Add(key, value);
        });

        public void RemoveKey(Key key) => Modify(dict => dict.Remove(key));
        public bool RemoveKeysByValue(Value value)
        {
            if (_collection.ContainsValue(value))
            {
                Modify(dict =>
                {
                    HashSet<Key> keys = new();

                    foreach (var pair in _collection)
                        if (value.Equals(pair.Value))
                            keys.Add(pair.Key);

                    foreach (var key in keys)
                        _collection.Remove(key);
                });
                return true;
            }
            return false;
        }

        public override void _Clear()
        {
            _collection.Clear();
        }
    }
}