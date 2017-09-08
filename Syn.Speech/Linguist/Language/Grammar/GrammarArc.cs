using System.Diagnostics;
//REFACTORED
namespace Syn.Speech.Linguist.Language.Grammar
{
    /// <summary>
    /// Represents a single transition out of a grammar node. The grammar represented is a stochastic grammar, each
    /// transition has a probability associated with it. The probabilities are relative and are not necessarily constrained
    /// to total 1.0.
    ///
    /// Note that all probabilities are maintained in the LogMath log base
    /// </summary>
    public class GrammarArc
    {
        /**
        /// Create a grammar arc
         *
        /// @param grammarNode    the node that this arc points to
        /// @param logProbability the log probability of following this arc
         */
        public GrammarArc(GrammarNode grammarNode, float logProbability) 
        {
            Debug.Assert(grammarNode != null);
            GrammarNode = grammarNode;
            Probability = logProbability;
        }


        /**
       /// Retrieves the destination node for this transition
        *
       /// @return the destination node
        */

        public GrammarNode GrammarNode { get; private set; }


        /**
       /// Retrieves the probability for this transition
        *
       /// @return the log probability for this transition
        */

        public float Probability { get; private set; }
    }
}
