using System;
using System.Collections;
using System.Collections.Generic;

namespace Syn.Speech.Helper
{
    internal class EnumeratorWrapper<T> : IEnumerator<T>
    {
        readonly object collection;
        IEnumerator<T> e;
        T lastVal;
        bool more;
        bool copied;

        public EnumeratorWrapper(object collection, IEnumerator<T> e)
        {
            this.e = e;
            this.collection = collection;
            more = e.MoveNext();
        }


        public void remove()
        {
            ICollection<T> col = collection as ICollection<T>;
            if (col == null)
            {
                throw new NotSupportedException();
            }
            if (more && !copied)
            {
                // Read the remaining elements, since the current enumerator
                // will be invalid after removing the element
                List<T> remaining = new List<T>();
                do
                {
                    remaining.Add(e.Current);
                } while (e.MoveNext());
                e = remaining.GetEnumerator();
                e.MoveNext();
                copied = true;
            }
            col.Remove(lastVal);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            return more;
        }

        public void Reset()
        {
            //throw new NotImplementedException();
        }

        public T Current
        {
            get
            {
                if (!more)
                    throw new NoSuchElementException();
                lastVal = e.Current;
                more = e.MoveNext();
                return lastVal;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
