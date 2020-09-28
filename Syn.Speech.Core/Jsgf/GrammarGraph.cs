using Syn.Speech.Linguist.Language.Grammar;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf
{
    /// <summary>
    /// Represents a graph of grammar nodes. A grammar graph has a single starting node and a single ending node.
    /// </summary>
    public class GrammarGraph
    {
        /// <summary>
        /// Nested Class support.
        /// </summary>
        private readonly JSGFGrammar _parent;

        /// <summary>
        /// Creates a grammar graph with the given nodes.
        /// </summary>
        /// <param name="startNode">The staring node of the graph.</param>
        /// <param name="endNode">The ending node of the graph.</param>
        /// <param name="parent">The parent.</param>
        internal GrammarGraph(GrammarNode startNode, GrammarNode endNode, JSGFGrammar parent)
        {
            _parent = parent;
            StartNode = startNode;
            EndNode = endNode;
        }

        /// <summary>
        /// Creates a graph with non-word nodes for the start and ending nodes.
        /// </summary>
        internal GrammarGraph(JSGFGrammar parent)
        {
            _parent = parent;
            StartNode = _parent.CreateGrammarNode(false);
            EndNode = _parent.CreateGrammarNode(false);
        }

        /// <summary>
        /// Gets the starting node
        /// </summary>
        /// <value>The starting node for the graph.</value>
        public GrammarNode StartNode { get; private set; }

        /// <summary>
        /// Gets the ending node.
        /// </summary>
        /// <value>The ending node for the graph.</value>
        public GrammarNode EndNode { get; private set; }
    }
}