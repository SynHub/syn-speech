using System;
using System.Collections.Generic;
using Syn.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    /// <summary>
    /// A list of ActiveLists. Different token types are placed in different lists.
    ///
    /// </summary>
    public class SimpleActiveListManager : ActiveListManager
    {
        /// <summary>
        /// This property is used in the Iterator returned by the getNonEmittingListIterator() method. When the
        /// Iterator.next() method is called, this property determines whether the lists prior to that returned by next() are
        /// empty (they should be empty). If they are not empty, an Error will be thrown.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropCheckPriorListsEmpty = "checkPriorListsEmpty";

        /// <summary>
        /// The property that defines the name of the active list factory to be used by this search manager.
        /// </summary>
        [S4ComponentList(Type = typeof(ActiveListFactory))]
        public static string PropActiveListFactories = "activeListFactories";

        // --------------------------------------
        // Configuration data
        // --------------------------------------
        internal Boolean CheckPriorLists;
        private List<ActiveListFactory> _activeListFactories;
        internal ActiveList[] CurrentActiveLists;

        public SimpleActiveListManager(List<ActiveListFactory> activeListFactories, Boolean checkPriorLists)
        {
            _activeListFactories = activeListFactories;
            CheckPriorLists = checkPriorLists;
        }

        public SimpleActiveListManager()
        {

        }

        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */
        public override void NewProperties(PropertySheet ps)
        {

            _activeListFactories = ps.GetComponentList<ActiveListFactory>(PropActiveListFactories);
            CheckPriorLists = ps.GetBoolean(PropCheckPriorListsEmpty);
        }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.decoder.search.ActiveListManager#setNumStateOrder(java.lang.Class[])
        */
        public override void SetNumStateOrder(int numStateOrder)
        {
            // check to make sure that we have the correct
            // number of active list factories for the given search states
            CurrentActiveLists = new ActiveList[numStateOrder];

            if (_activeListFactories.Count == 0)
            {
                this.LogInfo("No active list factories configured");
                throw new Exception("No active list factories configured");
            }
            if (_activeListFactories.Count != CurrentActiveLists.Length)
            {
                this.LogInfo("Need " + CurrentActiveLists.Length +
                        " active list factories, found " +
                        _activeListFactories.Count);
            }
            CreateActiveLists();
        }


        /**
        /// Creates the emitting and non-emitting active lists. When creating the non-emitting active lists, we will look at
        /// their respective beam widths (eg, word beam, unit beam, state beam).
         */
        private void CreateActiveLists()
        {
            int nlists = _activeListFactories.Count;
            for (int i = 0; i < CurrentActiveLists.Length; i++)
            {
                int which = i;
                if (which >= nlists)
                {
                    which = nlists - 1;
                }
                ActiveListFactory alf = _activeListFactories[which];
                CurrentActiveLists[i] = alf.NewInstance();
                //this.LogDebug("Added new item {0} at index {1} for currentActiveList",currentActiveLists[i],i);
            }
        }


        /**
        /// Adds the given token to the list
         *
        /// @param token the token to add
         */
        public override void Add(Token token)
        {
            ActiveList activeList = FindListFor(token);
            if (activeList == null)
            {
                throw new Exception("Cannot find ActiveList for "
                        + token.SearchState.GetType().Name);
            }
            //this.LogDebug("Token '{0}' to activeList of Size {1}", token, activeList.size());
            activeList.Add(token);
        }


        /**
        /// Given a token find the active list associated with the token type
         *
        /// @param token
        /// @return the active list
         */
        private ActiveList FindListFor(Token token)
        {
            return CurrentActiveLists[token.SearchState.Order];
        }


        /**
        /// Replaces an old token with a new token
         *
        /// @param oldToken the token to replace (or null in which case, replace works like add).
        /// @param newToken the new token to be placed in the list.
         */
        //public override void replace(Token oldToken, Token newToken)
        //{
        //    ActiveList activeList = findListFor(oldToken);
        //    Debug.Assert(activeList != null);
        //    activeList.replace(oldToken, newToken);
        //}


        /**
        /// Returns the emitting ActiveList from the manager
         *
        /// @return the emitting ActiveList
         */
        public override ActiveList GetEmittingList()
        {
            ActiveList list = CurrentActiveLists[CurrentActiveLists.Length - 1];
            this.LogDebug("Returning a list of size :{0}", list.Size);
            return list;
        }


        /**
        /// Clears emitting list in manager
         */
        public override void ClearEmittingList()
        {
            ActiveList list = CurrentActiveLists[CurrentActiveLists.Length - 1];
            CurrentActiveLists[CurrentActiveLists.Length - 1] = list.NewInstance();

        }


        /**
        /// Returns an Iterator of all the non-emitting ActiveLists. The iteration order is the same as the search state
        /// order.
         *
        /// @return an Iterator of non-emitting ActiveLists
         */

        /** Outputs debugging info for this list manager */
        public override void Dump()
        {
            this.LogInfo("--------------------");
            foreach (ActiveList al in CurrentActiveLists)
            {
                DumpList(al);
            }
        }

        public override Iterator<ActiveList> GetNonEmittingListIterator()
        {
            return (new NonEmittingListIterator(this));
        }

        /**
           /// Dumps out debugging info for the given active list
            *
           /// @param al the active list to dump
            */
        private void DumpList(ActiveList al)
        {
            this.LogInfo("Size: " + al.Size + " Best token: " + al.GetBestToken());
        }

    }








}
