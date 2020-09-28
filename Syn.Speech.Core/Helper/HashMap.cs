using System.Collections.Generic;

namespace Syn.Speech.Helper
{
    public class HashMap<T, V> :Dictionary<T, V>
    {
        public HashMap()
        {
        }

        public HashMap(HashMap<T,V> source ) : base(source)
        {
           
        }

        public HashMap(int capacity) : base(capacity) { }

        V _nullValue;

        public new V Remove(T key)
        {
            V item;
            if (key == null)
            {
                item = _nullValue;
                _nullValue = default(V);
            }
            else
            {
                item = this[key];
                base.Remove(key);
            }
            return item;
        }

        public V Get(T key)
        {
            if (key == null)
                return _nullValue;
            V toReturn;
            TryGetValue(key, out toReturn);
            return toReturn;
        }

        public V Put(T key, V item)
        {
            if (key == null)
            {
                _nullValue = item;
                return default(V);
            }

            V output;
            if (TryGetValue(key, out output))
            {
                this[key] = item;
                return output;
            }
            Add(key, item);
            return default(V);
        }

        public int Size()
        {
            return Count;
        }


        public HashSet<T> KeySet()
        {
            return new HashSet<T>(base.Keys);
        }

        public void PutAll(HashMap<T,V> source)
        {
            foreach (var item in source)
            {
                Put(item.Key, item.Value);
            }
        }
    }
}
