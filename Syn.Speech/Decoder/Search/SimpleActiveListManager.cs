using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Common;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// A list of ActiveLists. Different token types are placed in different lists.
    ///
    /// </summary>
    public class SimpleActiveListManager : ActiveListManager
    {
         /**
        /// This property is used in the Iterator returned by the getNonEmittingListIterator() method. When the
        /// Iterator.next() method is called, this property determines whether the lists prior to that returned by next() are
        /// empty (they should be empty). If they are not empty, an Error will be thrown.
         */
        [S4Boolean(defaultValue = false)]
        public static String PROP_CHECK_PRIOR_LISTS_EMPTY = "checkPriorListsEmpty";
    
        /** The property that defines the name of the active list factory to be used by this search manager. */
        [S4ComponentList(type = typeof(ActiveListFactory))]
        public static String PROP_ACTIVE_LIST_FACTORIES = "activeListFactories";

        // --------------------------------------
        // Configuration data
        // --------------------------------------
        private Boolean checkPriorLists;
        private List<ActiveListFactory> activeListFactories;
        private ActiveList[] currentActiveLists;


        /**
        /// 
        /// @param activeListFactories
        /// @param checkPriorLists
         */
        public SimpleActiveListManager(List<ActiveListFactory> activeListFactories, Boolean checkPriorLists) 
        {
            this.activeListFactories = activeListFactories;
            this.checkPriorLists = checkPriorLists;
        }

        public SimpleActiveListManager() {
        
        }

        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */
        override public void newProperties(PropertySheet ps)
        {

            activeListFactories = ps.getComponentList<ActiveListFactory>(PROP_ACTIVE_LIST_FACTORIES);
            checkPriorLists = ps.getBoolean(PROP_CHECK_PRIOR_LISTS_EMPTY);
        }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.decoder.search.ActiveListManager#setNumStateOrder(java.lang.Class[])
        */
        override public void setNumStateOrder(int numStateOrder) 
        {
            // check to make sure that we have the correct
            // number of active list factories for the given search states
            currentActiveLists = new ActiveList[numStateOrder];

            if (activeListFactories.Count==0) 
            {
                Trace.WriteLine("No active list factories configured");
                throw new Exception("No active list factories configured");
            }
            if (activeListFactories.Count != currentActiveLists.Length) 
            {
                Trace.WriteLine("Need " + currentActiveLists.Length +
                        " active list factories, found " +
                        activeListFactories.Count);
            }
            createActiveLists();
        }


        /**
        /// Creates the emitting and non-emitting active lists. When creating the non-emitting active lists, we will look at
        /// their respective beam widths (eg, word beam, unit beam, state beam).
         */
        private void createActiveLists() 
        {
            int nlists = activeListFactories.Count;
            for (int i = 0; i < currentActiveLists.Length; i++) 
            {
                int which = i;
                if (which >= nlists) {
                    which = nlists - 1;
                }
                ActiveListFactory alf = activeListFactories[which];
                currentActiveLists[i] = alf.newInstance();
            }
        }


        /**
        /// Adds the given token to the list
         *
        /// @param token the token to add
         */
        override public void add(Token token) 
        {
            ActiveList activeList = findListFor(token);
            if (activeList == null) 
            {
                throw new Exception("Cannot find ActiveList for "
                        + token.getSearchState().GetType().Name);
            }
            activeList.add(token);
        }


        /**
        /// Given a token find the active list associated with the token type
         *
        /// @param token
        /// @return the active list
         */
        private ActiveList findListFor(Token token) 
        {
            return currentActiveLists[token.getSearchState().getOrder()];
        }


        /**
        /// Replaces an old token with a new token
         *
        /// @param oldToken the token to replace (or null in which case, replace works like add).
        /// @param newToken the new token to be placed in the list.
         */
        //override public void replace(Token oldToken, Token newToken)
        //{
        //    ActiveList activeList = findListFor(oldToken);
        //    Trace.Assert(activeList != null);
        //    activeList.replace(oldToken, newToken);
        //}


        /**
        /// Returns the emitting ActiveList from the manager
         *
        /// @return the emitting ActiveList
         */
        override public ActiveList getEmittingList() 
        {
            ActiveList list = currentActiveLists[currentActiveLists.Length - 1];
            return list;
        }

    
        /**
        /// Clears emitting list in manager
         */
	    override public void clearEmittingList() 
        {
            ActiveList list = currentActiveLists[currentActiveLists.Length - 1];
		    currentActiveLists[currentActiveLists.Length - 1] = list.newInstance();	
	    }

	
        /**
        /// Returns an Iterator of all the non-emitting ActiveLists. The iteration order is the same as the search state
        /// order.
         *
        /// @return an Iterator of non-emitting ActiveLists
         */

        /** Outputs debugging info for this list manager */
        override public void dump() 
        {
            Trace.WriteLine("--------------------");
            foreach (ActiveList al in currentActiveLists) 
            {
                dumpList(al);
            }
        }

        public override IEnumerator<ActiveList> getNonEmittingListIterator()
        {
            return (new NonEmittingListIterator(currentActiveLists, checkPriorLists));
        }

        /**
           /// Dumps out debugging info for the given active list
            *
           /// @param al the active list to dump
            */
        private void dumpList(ActiveList al) 
        {
            Trace.WriteLine("Size: " + al.size() + " Best token: " + al.getBestToken());
        }

    }




    


    
}
