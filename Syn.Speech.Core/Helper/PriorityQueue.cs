﻿using System.Collections.Generic;

namespace Syn.Speech.Helper
{
    public class PriorityQueue<T>
    {
        private readonly List<T> _data;
        private readonly IComparer<T> _comparer;

        public PriorityQueue(IComparer<T> theComparer)
        {
            this._comparer = theComparer;
            this._data = new List<T>();
        }

        public PriorityQueue(int capacity, IComparer<T> theComparer )
        {
            this._comparer = theComparer;
            this._data = new List<T>(capacity);
        }

        public void Enqueue(T item)
        {
            _data.Add(item);
            int ci = _data.Count - 1; // child index; start at end
            while (ci > 0)
            {
                int pi = (ci - 1) / 2; // parent index
                if (Compare(_data[ci], _data[pi]) >= 0) break; // child item is larger than (or equal) parent so we're done
                T tmp = _data[ci]; _data[ci] = _data[pi]; _data[pi] = tmp;
                ci = pi;
            }
        }

        public T Dequeue()
        {
            // assumes pq is not empty; up to calling code
            int li = _data.Count - 1; // last index (before removal)
            T frontItem = _data[0];   // fetch the front
            _data[0] = _data[li];
            _data.RemoveAt(li);

            --li; // last index (after removal)
            int pi = 0; // parent index. start at front of pq
            while (true)
            {
                int ci = pi * 2 + 1; // left child index of parent
                if (ci > li) break;  // no children so done
                int rc = ci + 1;     // right child
                if (rc <= li && Compare(_data[rc], _data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                    ci = rc;
                if (Compare(_data[pi],(_data[ci])) <= 0) break; // parent is smaller than (or equal to) smallest child so done
                T tmp = _data[pi]; _data[pi] = _data[ci]; _data[ci] = tmp; // swap parent and child
                pi = ci;
            }
            return frontItem;
        }


        private int Compare(T first, T second)
        {
            return this._comparer.Compare(first, second);
        }

        public T Peek()
        {
            T frontItem = _data[0];
            return frontItem;
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < _data.Count; ++i)
                s += _data[i].ToString() + " ";
            s += "count = " + _data.Count;
            return s;
        }

        public bool IsConsistent()
        {
            // is the heap property true for all data?
            if (_data.Count == 0) return true;
            int li = _data.Count - 1; // last index
            for (int pi = 0; pi < _data.Count; ++pi) // each parent index
            {
                int lci = 2 * pi + 1; // left child index
                int rci = 2 * pi + 2; // right child index

                if (lci <= li && Compare(_data[pi], _data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
                if (rci <= li && Compare(_data[pi], _data[rci]) > 0) return false; // check the right child too.
            }
            return true; // passed all checks
        } // IsConsistent

        public void Add(T nb)
        {
            this.Enqueue(nb);
        }

        public T Remove()
        {
            return Dequeue();
        }
    } // PriorityQueue

    ///// <summary>
    ///// Priority queue based on binary heap,
    ///// Elements with minimum priority dequeued first
    ///// </summary>
    ///// <typeparam name="TPriority">Type of priorities</typeparam>
    ///// <typeparam name="TValue">Type of values</typeparam>
    //public class PriorityQueue<TPriority, TValue> : ICollection<KeyValuePair<TPriority, TValue>>
    //{
    //    private readonly List<KeyValuePair<TPriority, TValue>> _baseHeap;
    //    private readonly IComparer<TPriority> _comparer;

    //    #region Constructors

    //    /// <summary>
    //    /// Initializes a new instance of priority queue with default initial capacity and default priority comparer
    //    /// </summary>
    //    public PriorityQueue()
    //        : this(Comparer<TPriority>.Default)
    //    {
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of priority queue with specified initial capacity and default priority comparer
    //    /// </summary>
    //    /// <param name="capacity">initial capacity</param>
    //    /// <param name="comparer"></param>
    //    public PriorityQueue(int capacity, NodeComparer comparer)
    //        : this(capacity, Comparer<TPriority>.Default)
    //    {
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of priority queue with specified initial capacity and specified priority comparer
    //    /// </summary>
    //    /// <param name="capacity">initial capacity</param>
    //    /// <param name="comparer">priority comparer</param>
    //    public PriorityQueue(int capacity, IComparer<TPriority> comparer)
    //    {
    //        if (comparer == null)
    //            throw new ArgumentNullException();

    //        _baseHeap = new List<KeyValuePair<TPriority, TValue>>(capacity);
    //        _comparer = comparer;
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of priority queue with default initial capacity and specified priority comparer
    //    /// </summary>
    //    /// <param name="comparer">priority comparer</param>
    //    public PriorityQueue(IComparer<TPriority> comparer)
    //    {
    //        if (comparer == null)
    //            throw new ArgumentNullException();

    //        _baseHeap = new List<KeyValuePair<TPriority, TValue>>();
    //        _comparer = comparer;
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of priority queue with specified data and default priority comparer
    //    /// </summary>
    //    /// <param name="data">data to be inserted into priority queue</param>
    //    public PriorityQueue(IEnumerable<KeyValuePair<TPriority, TValue>> data)
    //        : this(data, Comparer<TPriority>.Default)
    //    {
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of priority queue with specified data and specified priority comparer
    //    /// </summary>
    //    /// <param name="data">data to be inserted into priority queue</param>
    //    /// <param name="comparer">priority comparer</param>
    //    public PriorityQueue(IEnumerable<KeyValuePair<TPriority, TValue>> data, IComparer<TPriority> comparer)
    //    {
    //        if (data == null || comparer == null)
    //            throw new ArgumentNullException();

    //        _comparer = comparer;
    //        _baseHeap = new List<KeyValuePair<TPriority, TValue>>(data);
    //        // heapify data
    //        for (int pos = _baseHeap.Count / 2 - 1; pos >= 0; pos--)
    //            HeapifyFromBeginningToEnd(pos);
    //    }

    //    #endregion

    //    #region Merging

    //    /// <summary>
    //    /// Merges two priority queues
    //    /// </summary>
    //    /// <param name="pq1">first priority queue</param>
    //    /// <param name="pq2">second priority queue</param>
    //    /// <returns>resultant priority queue</returns>
    //    /// <remarks>
    //    /// source priority queues must have equal comparers,
    //    /// otherwise <see cref="InvalidOperationException"/> will be thrown
    //    /// </remarks>
    //    public static PriorityQueue<TPriority, TValue> MergeQueues(PriorityQueue<TPriority, TValue> pq1, PriorityQueue<TPriority, TValue> pq2)
    //    {
    //        if (pq1 == null || pq2 == null)
    //            throw new ArgumentNullException();
    //        if (pq1._comparer != pq2._comparer)
    //            throw new InvalidOperationException("Priority queues to be merged must have equal comparers");
    //        return MergeQueues(pq1, pq2, pq1._comparer);
    //    }

    //    /// <summary>
    //    /// Merges two priority queues and sets specified comparer for resultant priority queue
    //    /// </summary>
    //    /// <param name="pq1">first priority queue</param>
    //    /// <param name="pq2">second priority queue</param>
    //    /// <param name="comparer">comparer for resultant priority queue</param>
    //    /// <returns>resultant priority queue</returns>
    //    public static PriorityQueue<TPriority, TValue> MergeQueues(PriorityQueue<TPriority, TValue> pq1, PriorityQueue<TPriority, TValue> pq2, IComparer<TPriority> comparer)
    //    {
    //        if (pq1 == null || pq2 == null || comparer == null)
    //            throw new ArgumentNullException();
    //        // merge data
    //        PriorityQueue<TPriority, TValue> result = new PriorityQueue<TPriority, TValue>(pq1.Count + pq2.Count, pq1._comparer);
    //        result._baseHeap.AddRange(pq1._baseHeap);
    //        result._baseHeap.AddRange(pq2._baseHeap);
    //        // heapify data
    //        for (int pos = result._baseHeap.Count / 2 - 1; pos >= 0; pos--)
    //            result.HeapifyFromBeginningToEnd(pos);

    //        return result;
    //    }

    //    #endregion

    //    #region Priority queue operations

    //    /// <summary>
    //    /// Enqueues element into priority queue
    //    /// </summary>
    //    /// <param name="priority">element priority</param>
    //    /// <param name="value">element value</param>
    //    public void Enqueue(TPriority priority, TValue value)
    //    {
    //        Insert(priority, value);
    //    }

    //    /// <summary>
    //    /// Dequeues element with minimum priority and return its priority and value as <see cref="KeyValuePair{TPriority,TValue}"/> 
    //    /// </summary>
    //    /// <returns>priority and value of the dequeued element</returns>
    //    /// <remarks>
    //    /// Method throws <see cref="InvalidOperationException"/> if priority queue is empty
    //    /// </remarks>
    //    public KeyValuePair<TPriority, TValue> Dequeue()
    //    {
    //        if (!IsEmpty)
    //        {
    //            KeyValuePair<TPriority, TValue> result = _baseHeap[0];
    //            DeleteRoot();
    //            return result;
    //        }
    //        else
    //            throw new InvalidOperationException("Priority queue is empty");
    //    }

    //    /// <summary>
    //    /// Dequeues element with minimum priority and return its value
    //    /// </summary>
    //    /// <returns>value of the dequeued element</returns>
    //    /// <remarks>
    //    /// Method throws <see cref="InvalidOperationException"/> if priority queue is empty
    //    /// </remarks>
    //    public TValue DequeueValue()
    //    {
    //        return Dequeue().Value;
    //    }

    //    /// <summary>
    //    /// Returns priority and value of the element with minimun priority, without removing it from the queue
    //    /// </summary>
    //    /// <returns>priority and value of the element with minimum priority</returns>
    //    /// <remarks>
    //    /// Method throws <see cref="InvalidOperationException"/> if priority queue is empty
    //    /// </remarks>
    //    public KeyValuePair<TPriority, TValue> Peek()
    //    {
    //        if (!IsEmpty)
    //            return _baseHeap[0];
    //        else
    //            throw new InvalidOperationException("Priority queue is empty");
    //    }

    //    /// <summary>
    //    /// Returns value of the element with minimun priority, without removing it from the queue
    //    /// </summary>
    //    /// <returns>value of the element with minimum priority</returns>
    //    /// <remarks>
    //    /// Method throws <see cref="InvalidOperationException"/> if priority queue is empty
    //    /// </remarks>
    //    public TValue PeekValue()
    //    {
    //        return Peek().Value;
    //    }

    //    /// <summary>
    //    /// Gets whether priority queue is empty
    //    /// </summary>
    //    public bool IsEmpty
    //    {
    //        get { return _baseHeap.Count == 0; }
    //    }

    //    #endregion

    //    #region Heap operations

    //    private void ExchangeElements(int pos1, int pos2)
    //    {
    //        KeyValuePair<TPriority, TValue> val = _baseHeap[pos1];
    //        _baseHeap[pos1] = _baseHeap[pos2];
    //        _baseHeap[pos2] = val;
    //    }

    //    private void Insert(TPriority priority, TValue value)
    //    {
    //        KeyValuePair<TPriority, TValue> val = new KeyValuePair<TPriority, TValue>(priority, value);
    //        _baseHeap.Add(val);

    //        // heap[i] have children heap[2*i + 1] and heap[2*i + 2] and parent heap[(i-1)/ 2];

    //        // heapify after insert, from end to beginning
    //        HeapifyFromEndToBeginning(_baseHeap.Count - 1);
    //    }


    //    private int HeapifyFromEndToBeginning(int pos)
    //    {
    //        if (pos >= _baseHeap.Count) return -1;

    //        while (pos > 0)
    //        {
    //            int parentPos = (pos - 1) / 2;
    //            if (_comparer.Compare(_baseHeap[parentPos].Key, _baseHeap[pos].Key) > 0)
    //            {
    //                ExchangeElements(parentPos, pos);
    //                pos = parentPos;
    //            }
    //            else break;
    //        }
    //        return pos;
    //    }


    //    private void DeleteRoot()
    //    {
    //        if (_baseHeap.Count <= 1)
    //        {
    //            _baseHeap.Clear();
    //            return;
    //        }

    //        _baseHeap[0] = _baseHeap[_baseHeap.Count - 1];
    //        _baseHeap.RemoveAt(_baseHeap.Count - 1);

    //        // heapify
    //        HeapifyFromBeginningToEnd(0);
    //    }

    //    private void HeapifyFromBeginningToEnd(int pos)
    //    {
    //        if (pos >= _baseHeap.Count) return;

    //        // heap[i] have children heap[2*i + 1] and heap[2*i + 2] and parent heap[(i-1)/ 2];

    //        while (true)
    //        {
    //            // on each iteration exchange element with its smallest child
    //            int smallest = pos;
    //            int left = 2 * pos + 1;
    //            int right = 2 * pos + 2;
    //            if (left < _baseHeap.Count && _comparer.Compare(_baseHeap[smallest].Key, _baseHeap[left].Key) > 0)
    //                smallest = left;
    //            if (right < _baseHeap.Count && _comparer.Compare(_baseHeap[smallest].Key, _baseHeap[right].Key) > 0)
    //                smallest = right;

    //            if (smallest != pos)
    //            {
    //                ExchangeElements(smallest, pos);
    //                pos = smallest;
    //            }
    //            else break;
    //        }
    //    }

    //    #endregion

    //    #region ICollection<KeyValuePair<TPriority, TValue>> implementation

    //    /// <summary>
    //    /// Enqueus element into priority queue
    //    /// </summary>
    //    /// <param name="item">element to add</param>
    //    public void Add(KeyValuePair<TPriority, TValue> item)
    //    {
    //        Enqueue(item.Key, item.Value);
    //    }

    //    /// <summary>
    //    /// Clears the collection
    //    /// </summary>
    //    public void Clear()
    //    {
    //        _baseHeap.Clear();
    //    }

    //    /// <summary>
    //    /// Determines whether the priority queue contains a specific element
    //    /// </summary>
    //    /// <param name="item">The object to locate in the priority queue</param>
    //    /// <returns><c>true</c> if item is found in the priority queue; otherwise, <c>false.</c> </returns>
    //    public bool Contains(KeyValuePair<TPriority, TValue> item)
    //    {
    //        return _baseHeap.Contains(item);
    //    }

    //    /// <summary>
    //    /// Gets number of elements in the priority queue
    //    /// </summary>
    //    public int Count
    //    {
    //        get { return _baseHeap.Count; }
    //    }

    //    /// <summary>
    //    /// Copies the elements of the priority queue to an Array, starting at a particular Array index. 
    //    /// </summary>
    //    /// <param name="array">The one-dimensional Array that is the destination of the elements copied from the priority queue. The Array must have zero-based indexing. </param>
    //    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    //    /// <remarks>
    //    /// It is not guaranteed that items will be copied in the sorted order.
    //    /// </remarks>
    //    public void CopyTo(KeyValuePair<TPriority, TValue>[] array, int arrayIndex)
    //    {
    //        _baseHeap.CopyTo(array, arrayIndex);
    //    }

    //    /// <summary>
    //    /// Gets a value indicating whether the collection is read-only. 
    //    /// </summary>
    //    /// <remarks>
    //    /// For priority queue this property returns <c>false</c>.
    //    /// </remarks>
    //    public bool IsReadOnly
    //    {
    //        get { return false; }
    //    }

    //    /// <summary>
    //    /// Removes the first occurrence of a specific object from the priority queue. 
    //    /// </summary>
    //    /// <param name="item">The object to remove from the ICollection <(Of <(T >)>). </param>
    //    /// <returns><c>true</c> if item was successfully removed from the priority queue.
    //    /// This method returns false if item is not found in the collection. </returns>
    //    public bool Remove(KeyValuePair<TPriority, TValue> item)
    //    {
    //        // find element in the collection and remove it
    //        int elementIdx = _baseHeap.IndexOf(item);
    //        if (elementIdx < 0) return false;

    //        //remove element
    //        _baseHeap[elementIdx] = _baseHeap[_baseHeap.Count - 1];
    //        _baseHeap.RemoveAt(_baseHeap.Count - 1);

    //        // heapify
    //        int newPos = HeapifyFromEndToBeginning(elementIdx);
    //        if (newPos == elementIdx)
    //            HeapifyFromBeginningToEnd(elementIdx);

    //        return true;
    //    }

    //    /// <summary>
    //    /// Returns an enumerator that iterates through the collection.
    //    /// </summary>
    //    /// <returns>Enumerator</returns>
    //    /// <remarks>
    //    /// Returned enumerator does not iterate elements in sorted order.</remarks>
    //    public IEnumerator<KeyValuePair<TPriority, TValue>> GetEnumerator()
    //    {
    //        return _baseHeap.GetEnumerator();
    //    }

    //    /// <summary>
    //    /// Returns an enumerator that iterates through the collection.
    //    /// </summary>
    //    /// <returns>Enumerator</returns>
    //    /// <remarks>
    //    /// Returned enumerator does not iterate elements in sorted order.</remarks>
    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return this.GetEnumerator();
    //    }

    //    #endregion

    //}
}
