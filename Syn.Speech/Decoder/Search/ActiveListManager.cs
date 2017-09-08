using System;
using System.Collections.Generic;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// An active list is maintained as a sorted list
    /// </summary>
    public abstract class ActiveListManager:IConfigurable
    {
        /** The property that specifies the absolute word beam width */
        [S4Integer(defaultValue = 2000)]
        public static String PROP_ABSOLUTE_WORD_BEAM_WIDTH =
                "absoluteWordBeamWidth";

        /** The property that specifies the relative word beam width */
        [S4Double(defaultValue = 0.0)]
        public static String PROP_RELATIVE_WORD_BEAM_WIDTH =
                "relativeWordBeamWidth";

        /**
        /// Adds the given token to the list
         *
        /// @param token the token to add
         */
        public abstract void add(Token token);


        /**
        /// Replaces an old token with a new token
         *
        /// @param oldToken the token to replace (or null in which case, replace works like add).
        /// @param newToken the new token to be placed in the list.
         */
        //public abstract void replace(Token oldToken, Token newToken);


        /**
        /// Returns an Iterator of all the non-emitting ActiveLists. The iteration order is the same as the search state
        /// order.
         *
        /// @return an Iterator of non-emitting ActiveLists
         */
        public abstract IEnumerator<ActiveList> getNonEmittingListIterator();


        /**
        /// Returns the emitting ActiveList from the manager
         *
        /// @return the emitting ActiveList
         */
        public abstract ActiveList getEmittingList();

    
        /**
        /// Clears emitting list in manager
         */
        public abstract void clearEmittingList();

	
        /** Dumps out debug info for the active list manager */
        public abstract void dump();


        /**
        /// Sets the total number of state types to be managed
         *
        /// @param numStateOrder the total number of state types
         */
        public abstract void setNumStateOrder(int numStateOrder);


        public abstract void newProperties(PropertySheet ps);
    }
}
