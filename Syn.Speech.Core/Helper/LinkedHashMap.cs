using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Syn.Speech.Helper
{
    public class LinkedHashMap<T, V> : IEnumerable
    {
        private readonly List<KeyValuePair<T, V>> _list;
        private readonly Dictionary<T, V> _table;
        public LinkedHashMap()
        {
            _table = new Dictionary<T, V>();
            _list = new List<KeyValuePair<T, V>>();
        }
        public void Clear()
        {
            _table.Clear();
            _list.Clear();
        }
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }
        public bool ContainsKey(object name)
        {
            return _table.ContainsKey((T)name);
        }
        public LinkedHashMap<T, V> EntrySet()
        {
            return this;
        }
        public V Get(object key)
        {
            V local;
            _table.TryGetValue((T) key, out local);
            return local;
        }
        protected IEnumerator<KeyValuePair<T, V>> InternalGetEnumerator()
        {
            return _list.GetEnumerator();
        }
        public bool IsEmpty()
        {
            return (_table.Count == 0);
        }
        public V Put(T key, V value)
        {
            V old;
            if (_table.TryGetValue(key, out old))
            {
                int index = _list.FindIndex(p => p.Key.Equals(key));
                if (index != -1)
                    _list.RemoveAt(index);
            }
            _table[key] = value;
            _list.Add(new KeyValuePair<T, V>(key, value));
            return old;
        }
        public V Remove(object key)
        {
            V local;
            if (_table.TryGetValue((T)key, out local))
            {
                int index = _list.FindIndex(p => p.Key.Equals(key));
                if (index != -1)
                    _list.RemoveAt(index);
                _table.Remove((T)key);
            }
            return local;
        }
        public IEnumerable<T> Keys
        {
            get { return _list.Select(p => p.Key); }
        }
        public IEnumerable<V> Values
        {
            get { return _list.Select(p => p.Value); }
        }

        public IEnumerator GetEnumerator()
        {
           return _list.GetEnumerator();
        }
    }

}
