using System;
using System.Collections.Generic;
using System.Text;
using Syn.Logging;
using Syn.Speech.Linguist.Dictionary;
//REFACTORED
namespace Syn.Speech.Linguist.Language.Grammar
{
    /// <summary>
    /// Represents a grammar node in a grammar. A {@link Grammar grammar} is represented as a graph of grammar nodes and
    /// {@link GrammarArc arcs}. A grammar node usually represents a word or words, but it can also be a transition point or
    /// simply silence.
    ///
    /// Note that all probabilities are maintained in the LogMath log base
    /// </summary>
    public class GrammarNode
    {
        private List<GrammarArc> _arcList = new List<GrammarArc>();      // arcs to successors


        /**
        /// Creates a GrammarNode with the given ID, Words. A GrammarNode with words is, by default, neither a silence nor a
        /// final node.
         *
        /// @param id           the identity of this GrammarNode
        /// @param alternatives the set of Words in this GrammarNode. This is a two dimensional array, the first index
        ///                     corresponds to the set of alternative choices, the second index corresponds to a particular
        ///                     word for the alternative
         */
        public GrammarNode(int id, Word[][] alternatives) 
            :this(id, false)
        {
            Alternatives = alternatives;
        }

        /// <summary>
        /// Creates a GrammarNode with the given ID and silence or final attributes. A silence or final node does not have
        /// any words by default.
        /// </summary>
        /// <param name="id">the identity of this GrammarNode</param>
        /// <param name="isFinal">if true this is a final node</param>
        public GrammarNode(int id, Boolean isFinal) 
        {
            ID = id;
            IsFinalNode = isFinal;
            Alternatives = new Word[0][];
        }

        /**
        /// Returns the ID of this GrammarNode.
         *
        /// @return the ID of this GrammarNode
         */

        public int ID { get; private set; }


        /**
        /// Retrieves the words associated with this grammar node
         *
        /// @return the words associated with this grammar node
         */

        public Word[][] Alternatives { get; private set; }


        /** Optimize this grammar node. */
        public void Optimize() 
        {
            for (int i = 0; i < _arcList.Count; i++) 
            {
                GrammarArc arc = _arcList[i];
                _arcList[i]= OptimizeArc(arc);
            }

            // now remove all self-looping empty arcs

            if (IsEmpty) 
            {
                for(Int16 i=0;i<_arcList.Count;i++) 
                {
                    if(i>_arcList.Count-2)
                        break;
                    GrammarArc arc = _arcList[i+1];
                    if (this == arc.GrammarNode) 
                        _arcList.RemoveAt(i);


                }
            }
        }


        /**
        /// Optimize the given arc. If an arc branches to an empty node that has only one exit, the node can be bypassed by
        /// making a new arc that skips the nodes. This can happen multiple times.
         *
        /// @param arc the arc to optimize
        /// @return the optimized arc
         */
        GrammarArc OptimizeArc(GrammarArc arc) 
        {
            GrammarNode nextNode = arc.GrammarNode;
            while (nextNode.IsEmpty && nextNode._arcList.Count == 1) 
            {
                GrammarArc nextArc = nextNode._arcList[0];
                arc = new GrammarArc(nextArc.GrammarNode,
                        arc.Probability + nextArc.Probability);
                nextNode = arc.GrammarNode;
            }
            return arc;
        }


        /**
        /// Retrieves the words associated with a specific alternative
         *
        /// @param alternative the index of the alternative
        /// @return the words associated with this grammar node
         */
        public Word[] GetWords(int alternative) 
        {
            return Alternatives[alternative];
        }


        /**
        /// Retrieve the single word associated with this grammar
         *
        /// @return the word associated with this grammar node
         */
        public Word GetWord() 
        {
            return Alternatives[0][0];
        }


        /**
        /// Gets the number of alternatives
         *
        /// @return the number of alternatives
         */
        public int GetNumAlternatives() 
        {
            return Alternatives.Length;
        }


        /**
        /// Determines if this grammar node is empty (that is, has no words).
         *
        /// @return <code>true</code> if the node is empty, otherwise <code>false</code>.
         */

        public bool IsEmpty
        {
            get { return GetNumAlternatives() == 0; }
        }


        /**
        /// Retrieves the set of transitions out of this node
         *
        /// @return the transitions to the successors for this node.
         */
        public GrammarArc[] GetSuccessors() 
        {
            return _arcList.ToArray();
        }


        /**
        /// Determines if this grammar node is a final node in the grammar
         *
        /// @return true if the node is a final node in the grammar
         */

        public bool IsFinalNode { get; private set; }


        /**
        /// Sets the 'final' state of the grammar node.  A 'final' state grammar marks the end of a grammar
         *
        /// @param isFinal if <code>true</code> the grammar node is a final node.
         */

        public void SetFinalNode(Boolean isFinal) 
        {
            IsFinalNode = isFinal;
        }


        /**
        /// Adds an arc to the given node
         *
        /// @param node           the node that this new arc goes to
        /// @param logProbability the log probability of the transition occuring
         */
        public void Add(GrammarNode node, float logProbability) 
        {
            // if we are an empty node, a loopback makes no sense.
            // this construct can be generated when dealing with recursive
            // grammars, so we check for them and toss them out.
            //
            if (IsEmpty && this == node) {
                return;
            }
            _arcList.Add(new GrammarArc(node, logProbability));
        }


        /** Returns the string representation of this object */
        override  public string ToString() 
        {
            return "G" + ID;
        }


        /**
        /// Dumps this GrammarNode as a String.
         *
        /// @param level        the indent level
        /// @param visitedNodes the set of visited nodes
        /// @param logProb      the probability of the transition (in logMath log domain)
         */
        private string Traverse(int level, HashSet<GrammarNode> visitedNodes, float logProb) 
        {
            StringBuilder dump = new StringBuilder();

            for (int i = 0; i < level; i++) {
                dump.Append("    ");
            }

            dump.Append("N(").Append(ID).Append("):");
            dump.Append("p:").Append(logProb);

            if (IsFinalNode) {
                dump.Append(" !");
            }

            Word[][] alternatives = Alternatives;
            for (int i = 0; i < alternatives.Length; i++) 
            {
                for (int j = 0; j < alternatives[i].Length; j++) 
                {
                    dump.Append(' ').Append(alternatives[i][j].Spelling);
                }
                if (i < alternatives.Length - 1) 
                {
                    dump.Append('|');
                }
            }

            this.LogInfo(dump);

            // Visit the children nodes if this node has never been visited.

            if (!IsFinalNode && !(visitedNodes.Contains(this))) 
            {
                visitedNodes.Add(this);
                GrammarArc[] arcs = GetSuccessors();

                foreach (GrammarArc arc in arcs) 
                {
                    GrammarNode child = arc.GrammarNode;
                    child.Traverse(level + 1, visitedNodes, arc.Probability);
                }
            } 
            else if (IsFinalNode) {

                // this node has no children, so just add it to the visitedNodes
                visitedNodes.Add(this);
            }

            return dump.ToString();
        }


        /**
        /// Traverse the grammar and dump out the nodes and arcs in GDL
         *
        /// @param out          print the gdl to this file
        /// @param visitedNodes the set of visited nodes
        /// @throws IOException if an error occurs while writing the file
         */
        private void TraverseGDL(HashSet<GrammarNode> visitedNodes)
        {

            // Visit the children nodes if this node has never been visited.

            if (!(visitedNodes.Contains(this))) 
            {
                visitedNodes.Add(this);
                this.LogInfo("   node: { title: " + GetGDLID(this) +
                        " label: " + GetGDLLabel(this) +
                        " shape: " + GetGDLShape(this) +
                        " color: " + GetGDLColor(this) + '}');
                GrammarArc[] arcs = GetSuccessors();
                foreach (GrammarArc arc in arcs) 
                {
                    GrammarNode child = arc.GrammarNode;
                    float prob = arc.Probability;
                    this.LogInfo("   edge: { source: "
                        + GetGDLID(this) +
                        " target: " + GetGDLID(child) +
                        " label: \"" + prob + "\"}");
                    child.TraverseGDL(visitedNodes);
                }
            }
        }


        /**
        /// Gvien a node, return a GDL ID for the node
         *
        /// @param node the node
        /// @return the GDL id
         */
        string GetGDLID(GrammarNode node) 
        {
            return "\"" + node.ID + "\"";
        }


        /**
        /// Given a node, returns a GDL Label for the node
         *
        /// @param node the node
        /// @return a gdl label for the node
         */
        string GetGDLLabel(GrammarNode node) 
        {
            string label = node.IsEmpty ? "" : node.GetWord().Spelling;
            return "\"" + label + "\"";
        }


        /**
        /// Given a node, returns a GDL shape for the node
         *
        /// @param node the node
        /// @return a gdl shape for the node
         */
        string GetGDLShape(GrammarNode node) 
        {
            return node.IsEmpty ? "circle" : "box";
        }


        /**
        /// Gets the color for the grammar node
         *
        /// @param node the node of interest
        /// @return the gdl label for the color
         */
        string GetGDLColor(GrammarNode node) 
        {
            string color = "grey";
            if (node.IsFinalNode) 
            {
                color = "red";
            } 
            else if (!node.IsEmpty) 
            {
                color = "green";
            }
            return color;
        }


        /**
        /// Dumps the grammar in GDL form
         *
        /// @param path the path to write the gdl file to
         */
        public void DumpGDL(String path) 
        {
            try {
                
                this.LogInfo("graph: {");
                this.LogInfo("    orientation: left_to_right");
                this.LogInfo("    layout_algorithm: dfs");
                TraverseGDL(new HashSet<GrammarNode>());
                this.LogInfo("}");
            }
            catch (Exception ioe) 
            {
                this.LogInfo("Trouble writing to " + path + ' ' + ioe);
            }
        }


        /** Dumps the grammar */
        public void Dump() 
        {
            this.LogInfo(Traverse(0, new HashSet<GrammarNode>(), 1.0f));
        }


        /// <summary>
        /// Splits this node into a pair of nodes. The first node in the pair retains the word info, and a single branch to
        /// the new second node. The second node retains all of the original successor branches.
        /// </summary>
        /// <param name="id">the id of the new node</param>
        /// <returns>the newly created second node.</returns>
        public GrammarNode SplitNode(int id) 
        {
            GrammarNode branchNode = new GrammarNode(id, false);
            branchNode._arcList = _arcList;
            _arcList = new List<GrammarArc>();
            Add(branchNode, 0.0f);
            return branchNode;
        }

	    public void DumpDot(String path) 
        {		
            try {
                this.LogInfo("digraph \"" + path + "\" {");
                this.LogInfo("rankdir = LR\n");
                TraverseDot(new HashSet<GrammarNode>());
                this.LogInfo("}");
               
            } 
            catch (Exception fnfe) 
            {
                this.LogInfo("Can't write to " + path + ' ' + fnfe);
            } 
	    }


	    private void TraverseDot(HashSet<GrammarNode> visitedNodes) 
        {
            if (!(visitedNodes.Contains(this))) 
            {
                visitedNodes.Add(this);
                this.LogInfo("\tnode" + ID 
                        + " [ label=" + GetGDLLabel(this) 
                        + ", color=" + GetGDLColor(this) 
                        + ", shape=" + GetGDLShape(this) 
                        + " ]\n");            		
                GrammarArc[] arcs = GetSuccessors();
                foreach (GrammarArc arc in arcs) 
                {
                    GrammarNode child = arc.GrammarNode;
                    float prob = arc.Probability;                
                    this.LogInfo("\tnode" + ID + " -> node" + child.ID 
                            + " [ label=" + prob + " ]\n");               
                    child.TraverseDot(visitedNodes);
                }
            }
	    }

    }
}
