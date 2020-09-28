using Syn.Speech.Linguist.Language.Grammar;
//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// Represents a non-emitting sentence hmm state
    /// </summary>
    public class GrammarState : SentenceHMMState
    {
        /**
        /// Creates a GrammarState
         *
        /// @param node the GrammarNode associated with this state
         */
        public GrammarState(GrammarNode node) 
            :base("G", null, node.ID)
        {
            GrammarNode = node;
            SetFinalState(GrammarNode.IsFinalNode);
        }


        /**
        /// Gets the grammar node associated with this state
         *
        /// @return the grammar node
         */

        public GrammarNode GrammarNode { get; private set; }


        /**
        /// Retrieves a short label describing the type of this state. Typically, subclasses of SentenceHMMState will
        /// implement this method and return a short (5 chars or less) label
         *
        /// @return the short label.
         */

        public override string TypeLabel
        {
            get { return "Gram"; }
        }


        /**
        /// Returns the state order for this state type
         *
        /// @return the state order
         */

        public override int Order
        {
            get { return 3; }
        }
    }
}
