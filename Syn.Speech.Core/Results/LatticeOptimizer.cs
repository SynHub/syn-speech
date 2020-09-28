using System;
using System.Collections.Generic;
using System.Diagnostics;
//REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    /// Class used to collapse all equivalent paths in a Lattice.  Results in a Lattices that is deterministic (no Node has
    /// Edges to two or more equivalent Nodes), and minimal (no Node has Edge from two or more equivalent Nodes).
    /// </summary>
    public class LatticeOptimizer
    {
        protected Lattice Lattice;

        /**
        /// Create a new Lattice optimizer
         *
        /// @param lattice
         */
        public LatticeOptimizer(Lattice lattice) {
            this.Lattice = lattice;
        }


        /**
        /// Code for optimizing Lattices.  An optimal lattice has all the same paths as the original, but with fewer nodes
        /// and edges
        /// <p/>
        /// Note that these methods are all in Lattice so that it is easy to change the definition of "equivalent" nodes and
        /// edges.  For example, an equivalent node might have the same word, but start or end at a different time.
        /// <p/>
        /// To experiment with other definitions of equivalent, just create a superclass of Lattice.
         */
        public void Optimize() {
            //System.err.println("***");
            //lattice.dumpAllPaths();
            //System.err.println("***");

            OptimizeForward();

            //System.err.println("***");
            //lattice.dumpAllPaths();
            //System.err.println("***");

            OptimizeBackward();

            //System.err.println("***");
            //lattice.dumpAllPaths();
            //System.err.println("***");

        }


        /**
        /// Make the Lattice deterministic, so that no node has multiple outgoing edges to equivalent nodes.
        /// <p/>
        /// Given two edges from the same node to two equivalent nodes, replace with one edge to one node with outgoing edges
        /// that are a union of the outgoing edges of the old two nodes.
        /// <p/>
        /// A --> B --> C \--> B' --> Y
        /// <p/>
        /// where B and B' are equivalent.
        /// <p/>
        /// is replaced with
        /// <p/>
        /// A --> B" --> C \--> Y
        /// <p/>
        /// where B" is the merge of B and B'
        /// <p/>
        /// Note that equivalent nodes must have the same incomming edges. For example
        /// <p/>
        /// A --> B \ \ X --> B'
        /// <p/>
        /// B and B' would not be equivalent because the incomming edges are different
         */
        protected void OptimizeForward() {
            //System.err.println("*** Optimizing forward ***");

            var moreChanges = true;
            while (moreChanges) {
                moreChanges = false;
                // search for a node that can be optimized
                // note that we use getCopyOfNodes to avoid concurrent changes to nodes
                foreach (var n in Lattice.GetCopyOfNodes()) 
                {
                    // we are iterating down a list of node before optimization
                    // previous iterations may have removed nodes from the list
                    // therefore we have to check that the node stiff exists
                    if (Lattice.HasNode(n)) 
                    {
                        moreChanges |= OptimizeNodeForward(n);
                    }
                }
            }
        }


        /**
        /// Look for 2 "to" edges to equivalent nodes.  Replace the edges with one edge to one node that is a merge of the
        /// equivalent nodes
        /// <p/>
        /// nodes are equivalent if they have equivalent from edges, and the same label
        /// <p/>
        /// merged nodes have a union of "from" and "to" edges
         *
        /// @param n
        /// @return true if Node n required an optimize forward
         */
        protected Boolean OptimizeNodeForward(Node n) 
        {
            Debug.Assert(Lattice.HasNode(n));

            var leavingEdges = new List<Edge>(n.LeavingEdges);
            for (var j = 0; j < leavingEdges.Count; j++) 
            {
                var e = leavingEdges[j];
                for (var k = j + 1; k < leavingEdges.Count; k++) 
                {
                    var e2 = leavingEdges[k];

                    /*
                    /// If these are not the same edge, and they point to
                    /// equivalent nodes, we have a hit, return true
                     */
                    Debug.Assert(e != e2);
                    if (EquivalentNodesForward(e.ToNode, e2.ToNode)) 
                    {
                        MergeNodesAndEdgesForward(n, e, e2);
                        return true;
                    }
                }
            }
            /*
            /// return false if we did not get a hit
             */
            return false;
        }


        /**
        /// nodes are equivalent forward if they have "from" edges from the same nodes, and have equivalent labels (Token,
        /// start/end times)
         *
        /// @param n1
        /// @param n2
        /// @return true if n1 and n2 are "equivalent forwards"
         */
        protected Boolean EquivalentNodesForward(Node n1, Node n2) 
        {

            Debug.Assert(Lattice.HasNode(n1));
            Debug.Assert(Lattice.HasNode(n2));

            // do the labels match?
            if (!EquivalentNodeLabels(n1, n2)) return false;

            // if they have different number of "from" edges they are not equivalent
            // or if there is a "from" edge with no match then the nodes are not
            // equivalent
            return n1.HasEquivalentEnteringEdges(n2);
        }


        /**
        /// given edges e1 and e2 from node n to nodes n1 and n2
        /// <p/>
        /// merge e1 and e2, that is, merge the scores of e1 and e2 create n' that is a merge of n1 and n2 add n' add edge e'
        /// from n to n'
        /// <p/>
        /// remove n1 and n2 and all associated edges
         *
        /// @param n
        /// @param e1
        /// @param e2
         */
        protected void MergeNodesAndEdgesForward(Node n, Edge e1, Edge e2) 
        {
            Debug.Assert(Lattice.HasNode(n));
            Debug.Assert(Lattice.HasEdge(e1));
            Debug.Assert(Lattice.HasEdge(e2));

            Debug.Assert(e1.FromNode == n);
            Debug.Assert(e2.FromNode == n);

            var n1 = e1.ToNode;
            var n2 = e2.ToNode;

            Debug.Assert(n1.HasEquivalentEnteringEdges(n1));
            Debug.Assert(n1.Word.Equals(n2.Word));

            // merge the scores of e1 and e2 into e1
            e1.AcousticScore = MergeAcousticScores
                (e1.AcousticScore, e2.AcousticScore);
            e1.LMScore = MergeLanguageScores(e1.LMScore,
                e2.LMScore);

            // add n2's edges to n1
            foreach (var e in n2.LeavingEdges) 
            {
                e2 = n1.GetEdgeToNode(e.ToNode);
                if (e2 == null) {
                    Lattice.AddEdge(n1, e.ToNode,
                            e.AcousticScore, e.LMScore);
                } else {
                    // if we got here then n1 and n2 had edges to the same node
                    // choose the edge with best score
                    e2.AcousticScore = MergeAcousticScores
                        (e.AcousticScore, e2.AcousticScore);
                    e2.LMScore = MergeLanguageScores(e.LMScore,
                        e2.LMScore);
                }
            }

            // remove n2 and all associated edges
            Lattice.RemoveNodeAndEdges(n2);
        }


        /**
        /// Minimize the Lattice deterministic, so that no node has multiple incoming edges from equivalent nodes.
        /// <p/>
        /// Given two edges from equivalent nodes to a single nodes, replace with one edge from one node with incoming edges
        /// that are a union of the incoming edges of the old two nodes.
        /// <p/>
        /// A --> B --> C X --> B' --/
        /// <p/>
        /// where B and B' are equivalent.
        /// <p/>
        /// is replaced with
        /// <p/>
        /// A --> B" --> C X --/
        /// <p/>
        /// where B" is the merge of B and B'
        /// <p/>
        /// Note that equivalent nodes must have the same outgoing edges. For example
        /// <p/>
        /// A --> X \ \ \ A' --> B
        /// <p/>
        /// A and A' would not be equivalent because the outgoing edges are different
         */
        protected void OptimizeBackward() {
            //System.err.println("*** Optimizing backward ***");

            var moreChanges = true;
            while (moreChanges) {
                moreChanges = false;
                // search for a node that can be optimized
                // note that we use getCopyOfNodes to avoid concurrent changes to nodes
                foreach (var n in Lattice.GetCopyOfNodes()) {
                    // we are iterating down a list of node before optimization
                    // previous iterations may have removed nodes from the list
                    // therefore we have to check that the node stiff exists
                    if (Lattice.HasNode(n)) {
                        moreChanges |= OptimizeNodeBackward(n);
                    }
                }
            }
        }


        /**
        /// Look for 2 entering edges from equivalent nodes.  Replace the edges with one edge to one new node that is a merge
        /// of the equivalent nodes Nodes are equivalent if they have equivalent to edges, and the same label. Merged nodes
        /// have a union of entering and leaving edges
         *
        /// @param n
        /// @return true if Node n required optimizing backwards
         */
        protected Boolean OptimizeNodeBackward(Node n) {
            var enteringEdges = new List<Edge>(n.EnteringEdges);
            for (var j = 0; j < enteringEdges.Count; j++) 
            {
                var e = enteringEdges[j];
                for (var k = j + 1; k < n.EnteringEdges.Count; k++) 
                {
                    var e2 = enteringEdges[k];

                    /*
                    /// If these are not the same edge, and they point to
                    /// equivalent nodes, we have a hit, return true
                     */
                    Debug.Assert(e != e2);
                    if (EquivalentNodesBackward(e.FromNode,
                            e2.FromNode)) 
                    {
                        MergeNodesAndEdgesBackward(n, e, e2);
                        return true;
                    }
                }
            }
            /*
            /// return false if we did not get a hit
             */
            return false;
        }


        /**
        /// nodes are equivalent backward if they have "to" edges to the same nodes, and have equivalent labels (Token,
        /// start/end times)
         *
        /// @param n1
        /// @param n2
        /// @return true if n1 and n2 are "equivalent backwards"
         */
        protected Boolean EquivalentNodesBackward(Node n1, Node n2) 
        {

            Debug.Assert(Lattice.HasNode(n1));
            Debug.Assert(Lattice.HasNode(n2));

            // do the labels match?
            if (!EquivalentNodeLabels(n1, n2)) return false;

            // if they have different number of "to" edges they are not equivalent
            // or if there is a "to" edge with no match then the nodes are not equiv
            return n1.HasEquivalentLeavingEdges(n2);
        }


        /**
        /// Is the contents of these Node equivalent?
         *
        /// @param n1
        /// @param n2
        /// @return true if n1 and n2 have "equivalent labels"
         */
        protected Boolean EquivalentNodeLabels(Node n1, Node n2) 
        {
            return (n1.Word.Equals(n2.Word) &&
                    (n1.BeginTime == n2.BeginTime &&
                            n1.EndTime == n2.EndTime));
        }


        /**
        /// given edges e1 and e2 to node n from nodes n1 and n2
        /// <p/>
        /// merge e1 and e2, that is, merge the scores of e1 and e2 create n' that is a merge of n1 and n2 add n' add edge e'
        /// from n' to n
        /// <p/>
        /// remove n1 and n2 and all associated edges
         *
        /// @param n
        /// @param e1
        /// @param e2
         */
        protected void MergeNodesAndEdgesBackward(Node n, Edge e1, Edge e2) {
            Debug.Assert(Lattice.HasNode(n));
            Debug.Assert(Lattice.HasEdge(e1));
            Debug.Assert(Lattice.HasEdge(e2));

            Debug.Assert(e1.ToNode == n);
            Debug.Assert(e2.ToNode == n);

            var n1 = e1.FromNode;
            var n2 = e2.FromNode;

            Debug.Assert(n1.HasEquivalentLeavingEdges(n2));
            Debug.Assert(n1.Word.Equals(n2.Word));

            // merge the scores of e1 and e2 into e1
            e1.AcousticScore = MergeAcousticScores(e1.AcousticScore,
                e2.AcousticScore);
            e1.LMScore = MergeLanguageScores(e1.LMScore,
                e2.LMScore);

            // add n2's "from" edges to n1
            foreach (var e in n2.EnteringEdges) 
            {
                e2 = n1.GetEdgeFromNode(e.FromNode);
                if (e2 == null) {
                    Lattice.AddEdge(e.FromNode, n1,
                            e.AcousticScore, e.LMScore);
                } else {
                    // if we got here then n1 and n2 had edges from the same node
                    // choose the edge with best score
                    e2.AcousticScore = MergeAcousticScores(e.AcousticScore,
                        e2.AcousticScore);
                    e2.LMScore = MergeLanguageScores(e.LMScore,
                        e2.LMScore);
                }
            }

            // remove n2 and all associated edges
            Lattice.RemoveNodeAndEdges(n2);
        }


        /** Remove all Nodes that have no Edges to them (but not <s>) */
        protected void RemoveHangingNodes() 
        {
            foreach (var n in Lattice.GetCopyOfNodes()) 
            {
                if (Lattice.HasNode(n)) {
                    if (n == Lattice.InitialNode) {

                    } else if (n == Lattice.TerminalNode) {

                    } else {
                        if (n.LeavingEdges.Count==0
                                || n.EnteringEdges.Count==0) 
                        {
                            Lattice.RemoveNodeAndEdges(n);
                            RemoveHangingNodes();
                            return;
                        }
                    }
                }
            }
        }


        /**
        /// Provides a single method to merge acoustic scores, so that changes to how acoustic score are merged can be made
        /// at one point only.
         *
        /// @param score1 the first acoustic score
        /// @param score2 the second acoustic score
        /// @return the merged acoustic score
         */
        private static double MergeAcousticScores(double score1, double score2) 
        {
            // return lattice.getLogMath().addAsLinear(score1, score2);
            return Math.Max(score1, score2);
        }


        /**
        /// Provides a single method to merge language scores, so that changes to how language score are merged can be made
        /// at one point only.
         *
        /// @param score1 the first language score
        /// @param score2 the second language score
        /// @return the merged language score
         */
        private static double MergeLanguageScores(double score1, double score2) 
        {
            // return lattice.getLogMath().addAsLinear(score1, score2);
            return Math.Max(score1, score2);
        }


        /**
        /// Self test for LatticeOptimizer
         *
        /// @param args
         */
        public static void Main(String[] args) 
        {
            var lattice = new Lattice(args[0]);

            var optimizer = new LatticeOptimizer(lattice);

            optimizer.Optimize();

            lattice.Dump(args[1]);
        }
    }
}
