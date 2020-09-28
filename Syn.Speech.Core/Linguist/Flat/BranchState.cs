using System;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    ///  Represents a branching node in a grammar
    /// </summary>
    class BranchState : SentenceHMMState
    {
        /**
        /// Creates a branch state
         *
        /// @param nodeID the grammar node id
         */
        public BranchState(String leftContext, string rightContext, int nodeID) 
            :base("B[" + leftContext + "," +
                    rightContext + "]", null, nodeID)
        {
        }


        /**
        /// Retrieves a short label describing the type of this state. Typically, subclasses of SentenceHMMState will
        /// implement this method and return a short (5 chars or less) label
         *
        /// @return the short label.
         */

        public override string TypeLabel
        {
            get { return "Brnch"; }
        }


        /**
        /// Returns the state order for this state type
         *
        /// @return the state order
         */

        public override int Order
        {
            get { return 2; }
        }
    }
}
