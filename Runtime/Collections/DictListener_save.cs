//using System;
//using System.Collections.Generic;

//namespace _UTIL_
//{
//    public class DictListener<Key, Value>
//    {
//        public readonly Dictionary<Key, Value> _dict;

//        public Type key_type = typeof(Key), value_type = typeof(Value);

//        public readonly EventPropagator<bool> _listeners1 = new();
//        public readonly EventPropagator<Dictionary<Key, Value>> _listeners2 = new(), _oneTimeListeners = new();

//        //--------------------------------------------------------------------------------------------------------------

//        public DictListener(in Dictionary<Key, Value> dict)
//        {
//            _dict = dict;
//        }

//        public DictListener() : this(new())
//        {
//        }

//        //--------------------------------------------------------------------------------------------------------------

//        public bool IsEmpty
//        {
//            get
//            {
//                lock (this)
//                    return _dict.Count == 0;
//            }
//        }

//        public bool IsNotEmpty
//        {
//            get
//            {
//                lock (this)
//                    return _dict.Count > 0;
//            }
//        }

//        void RemoveZombieValues()
//        {
//            if (_dict.Count > 0)
//            {
//                HashSet<Key> keys = null;

//                foreach (var pair in _dict)
//                    if (pair.Value == null)
//                        (keys ??= new()).Add(pair.Key);

//                if (keys != null && keys.Count > 0)
//                    ModifyDict(dict =>
//                    {
//                        foreach (var key in keys)
//                            dict.Remove(key);
//                    });
//            }
//        }

//        public void AddListener1(in object user, in Action<bool> action)
//        {
//            RemoveZombieValues();
//            lock (this)
//                _listeners1.AddListener(IsNotEmpty, user, action);
//        }


//        public void AddListener2(in object user, in Action<Dictionary<Key, Value>> action)
//        {
//            RemoveZombieValues();
//            lock (this)
//                _listeners2.AddListener(_dict, user, action);
//        }

//        public void AddOneTimeListener(in object user, in Action<Dictionary<Key, Value>> action)
//        {
//            RemoveZombieValues();
//            lock (this)
//                _oneTimeListeners.AddListener(_dict, user, action);
//        }

//        public void ModifyDict(in Action<Dictionary<Key, Value>> onDict)
//        {
//            lock (this)
//            {
//                int count1 = _dict.Count;
//                onDict(_dict);
//                RemoveZombieValues();
//                int count2 = _dict.Count;

//                _listeners2.NotifyListeners(_dict);

//                if (count1 != count2)
//                    if (count1 == 0 || count2 == 0)
//                        _listeners1.NotifyListeners(IsNotEmpty);

//                _oneTimeListeners.NotifyListeners(_dict);
//                _oneTimeListeners._listeners.Clear();
//            }
//        }

//        public void ClearList()
//        {
//            ModifyList(list => list.Clear());
//        }

//        public void Reset()
//        {
//            lock (this)
//            {
//                _listeners1.Dispose();
//                _listeners2.Dispose();
//                _oneTimeListeners.Dispose();
//                _list.Clear();
//            }
//        }
//    }
//}