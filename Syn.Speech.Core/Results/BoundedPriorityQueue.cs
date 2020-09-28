using System.Collections;
using System.Collections.Generic;
using System.Linq;
//REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    ///  // TODO: replace with MinMaxPriorityQueue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BoundedPriorityQueue<T> : IEnumerable<T>
    {
        readonly Queue<T> _items;
        readonly int _maxSize;

        public BoundedPriorityQueue(int maxSize)
        {
            _items = new Queue<T>();
            this._maxSize = maxSize;
        }

        public void Add(T item)
        {
            _items.Enqueue(item);
            if (_items.Count > _maxSize)
                _items.Dequeue();
        }

        public int Size()
        {
            return _items.Count;
        }

        public T Poll()
        {
            return _items.Last();
        }

        public IEnumerator<T> Iterator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
