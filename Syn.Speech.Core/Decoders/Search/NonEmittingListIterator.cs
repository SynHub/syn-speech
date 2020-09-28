using System;
using System.Collections;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    class NonEmittingListIterator : Iterator<ActiveList>
    {

        private readonly SimpleActiveListManager _parent;
        private int _listPtr;
        

        public NonEmittingListIterator() {
            _listPtr = -1;
           
        }

        public NonEmittingListIterator(SimpleActiveListManager parent):this()
        {
            _parent = parent;
        }


        public bool MoveNext()
        {
            return _listPtr + 1 < _parent.CurrentActiveLists.Length - 1;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }


        public ActiveList Current {
            get
            {
                _listPtr++;

                if (_listPtr >= _parent.CurrentActiveLists.Length)
                {
                    throw new Exception("NoSuchElementException");
                }
                if (_parent.CheckPriorLists)
                {
                    CheckPriorLists();
                }
                return _parent.CurrentActiveLists[_listPtr];
            }
        }


        /// <summary>
        /// Check that all lists prior to listPtr is empty.
        /// </summary>
        /// <exception cref="System.Exception">At while processing state order
        ///                             + _listPtr + , state order  + i +  not empty</exception>
        private void CheckPriorLists() {
            for (int i = 0; i < _listPtr; i++) {
                ActiveList activeList = _parent.CurrentActiveLists[i];
                if (activeList.Size > 0) {
                    throw new Exception("At while processing state order"
                            + _listPtr + ", state order " + i + " not empty");
                }
            }
        }


        public void Remove() {
            _parent.CurrentActiveLists[_listPtr] = _parent.CurrentActiveLists[_listPtr].NewInstance();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}