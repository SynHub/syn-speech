using System;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Language.Grammar
{
    /// <summary>
    /// Creates a grammar from a reference sentence. It is a constrained grammar that represents the sentence only.
    /// Note that all grammar probabilities are maintained in the LogMath log base.
    /// </summary>
    public class ForcedAlignerGrammar : Grammar
    {

        protected GrammarNode FinalNode;

        public ForcedAlignerGrammar(bool showGrammar, bool optimizeGrammar, bool addSilenceWords, bool addFillerWords, IDictionary dictionary)
            : base(showGrammar, optimizeGrammar, addSilenceWords, addFillerWords, dictionary)
        {

        }

        public ForcedAlignerGrammar()
        {

        }


        /// <summary>
        /// Create class from reference text (not implemented).
        /// </summary>
        /// <returns></returns>
        protected override GrammarNode CreateGrammar()
        {
            throw new Error("Not implemented");
        }


        /// <summary>
        /// Creates the grammar.
        /// </summary>
        protected override GrammarNode CreateGrammar(String referenceText)
        {

            InitialNode = CreateGrammarNode(false);
            FinalNode = CreateGrammarNode(true);
            CreateForcedAlignerGrammar(InitialNode, FinalNode, referenceText);

            return InitialNode;
        }


        /// <summary>
        /// Create a branch of the grammar that corresponds to a transcript.  
        /// For each word create a node, and link the nodes with arcs. 
        /// The branch is connected to the initial node iNode, and the final node fNode.
        /// </summary>
        /// <returns>The first node of this branch.</returns>
        protected GrammarNode CreateForcedAlignerGrammar(GrammarNode iNode, GrammarNode fNode, String transcript)
        {
            var logArcProbability = LogMath.LogOne;

            var tok = new StringTokenizer(transcript);

            GrammarNode firstNode = null;
            GrammarNode lastNode = null;

            while (tok.hasMoreTokens())
            {
                var token = tok.nextToken();

                var prevNode = lastNode;
                lastNode = CreateGrammarNode(token);
                if (firstNode == null) firstNode = lastNode;

                if (prevNode != null)
                {
                    prevNode.Add(lastNode, logArcProbability);
                }
            }

            iNode.Add(firstNode, logArcProbability);
            lastNode.Add(fNode, logArcProbability);

            return firstNode;
        }
    }

}
