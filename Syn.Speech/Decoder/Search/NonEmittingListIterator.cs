using System;
using System.Collections;
using System.Collections.Generic;
using Syn.Speech.Common;
using Syn.Speech.Helper;

namespace Syn.Speech.Decoder.Search
{
    class NonEmittingListIterator : IEnumerator<ActiveList> {

        private ActiveList[] currentActiveLists;
        private bool _checkPriorListsFromParent;

        private int listPtr;
        

        public NonEmittingListIterator() {
            listPtr = -1;
        }

        public NonEmittingListIterator(ActiveList[] currentActiveLists, bool checkPriorListsFromParent)
        {
            this.currentActiveLists = currentActiveLists;
            this._checkPriorListsFromParent = checkPriorListsFromParent;
        }


        public bool MoveNext()
        {
            return listPtr + 1 < currentActiveLists.Length - 1;
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
                listPtr++;

                if (listPtr >= currentActiveLists.Length)
                {
                    throw new Exception("NoSuchElementException");
                }
                if (_checkPriorListsFromParent)
                {
                    checkPriorLists();
                }
                return currentActiveLists[listPtr];
            }
        }


        /** Check that all lists prior to listPtr is empty. */
        private void checkPriorLists() {
            for (int i = 0; i < listPtr; i++) {
                ActiveList activeList = currentActiveLists[i] as ActiveList;
                if (activeList.size() > 0) {
                    throw new Exception("At while processing state order"
                            + listPtr + ", state order " + i + " not empty");
                }
            }
        }


        public void remove() {
            currentActiveLists[listPtr] =
                    currentActiveLists[listPtr].newInstance();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}