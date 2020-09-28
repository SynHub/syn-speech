using System.Collections.Generic;

namespace Syn.Speech.Helper
{
    public class CommonIterator<T>
    {
        private int _index = -1;
        private readonly List<T> _source; 

        public CommonIterator(IEnumerable<T> source)
        {
            _source = new List<T>(source);
        }

        public CommonIterator(List<T> source)
        {
            _source = source;
        }  

        public bool hasNext()
        {
            return _index < _source.Count;
        }

        public int nextIndex()
        {
            return _index + 1;
        }

        public int previousIndex()
        {
            return _index - 1;
        }

        public bool hasPrevious()
        {
            return _index > _source.Count;
        }

        public T next()
        {
            _index++;
            return _source[_index];
        }

        public T previous()
        {
            _index--;
            return _source[_index];
        }

        public void remove(T item)
        {
            _source.Remove(item);
        }

        public void set(T item)
        {
            _source[_index] = item;
        }

        public void add(T item)
        {
            _source.Insert(_index, item);
        }

        public List<T> Source
        {
            get { return _source; }   
        }

        public int Index { get { return _index; } set { _index = value; } }
    }
}
