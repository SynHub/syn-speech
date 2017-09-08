using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Syn.Speech.Helper
{
    namespace PrioQueue
    {
        public class JPriorityQueue <T>
        {
            int _totalSize;
            readonly SortedDictionary<int, Queue> _storage;

            public JPriorityQueue(IComparer<int> comparer)
            {
                _storage = new SortedDictionary<int, Queue>(comparer);
                this._totalSize = 0;
            }

            public bool IsEmpty()
            {
                return (_totalSize == 0);
            }

            public T Dequeue()
            {
                if (IsEmpty())
                {
                    throw new Exception("Please check that priorityQueue is not empty before dequeing");
                }
                else
                    foreach (var q in _storage.Values)
                    {
                        // we use a sorted dictionary
                        if (q.Count > 0)
                        {
                            _totalSize--;
                            return (T) q.Dequeue();
                        }
                    }

                Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

                return default(T); // not supposed to reach here.
            }

            // same as above, except for peek.

            public T Peek()
            {
                if (IsEmpty())
                    throw new Exception("Please check that priorityQueue is not empty before peeking");
                else
                    foreach (Queue q in _storage.Values)
                    {
                        if (q.Count > 0)
                            return (T) q.Peek();
                    }

                Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

                return default(T); // not supposed to reach here.
            }

            public T Dequeue(int prio)
            {
                _totalSize--;
                return (T) _storage[prio].Dequeue();
            }

            public void Enqueue(T item, int prio)
            {
                if (!_storage.ContainsKey(prio))
                {
                    _storage.Add(prio, new Queue());
                }
                _storage[prio].Enqueue(item);
                _totalSize++;

            }
        }
    }
}
