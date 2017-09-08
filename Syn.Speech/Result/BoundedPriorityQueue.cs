using System.Collections.Generic;
using System.Linq;

namespace Syn.Speech.Result
{
    /// <summary>
    ///  // TODO: replace with MinMaxPriorityQueue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BoundedPriorityQueue<T> : IEnumerable<T>
    {
        Queue<T> items;
        int maxSize;

        public BoundedPriorityQueue(int maxSize)
        {
            items = new Queue<T>();
            this.maxSize = maxSize;
        }

        public void add(T item)
        {
            items.Enqueue(item);
            if (items.Count > maxSize)
                items.Dequeue();
        }

        public int size()
        {
            return items.Count;
        }

        public T poll()
        {
            return items.Last();
        }

        public IEnumerator<T> iterator()
        {
            return items.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
