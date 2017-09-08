using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    /// 
    /**
    /// <p/>
    /// A node is part of Lattices, representing the theory that a word was spoken over a given period of time. A node also
    /// has a set of entering and leaving {@link edu.cmu.sphinx.result.Edge edges}, connecting it to other nodes. One can get
    /// and set the beginning and end frames of the word via the getBeginTime and getEndTime methods. When setting these
    /// times, the beginning time must be earlier or equal to the end time, otherwise an error will be thrown. </p>
    /// <p/>
    /// <p/>
    /// The posterior probability of any word in a word lattice is the probability that the node representing that word
    /// occurs on any path through the lattice. It is usually computed as the ratio of the total likelihood scores of all
    /// paths through the lattice that pass through the node, to the total likelihood score of all paths through the lattice.
    /// Path scores are usually computed using the acoustic likelihoods of the nodes, although language scores can also be
    /// incorporated. The posterior probabilities of an entire lattice is usually computed efficiently using the
    /// Forward-Backward Algorithm. Refer to the {@link edu.cmu.sphinx.result.Lattice#computeNodePosteriors
    /// computeNodePosteriors} method in the Lattice class for details. </p>
     */
    /// </summary>
    public class Node
    {
        // used to generate unique IDs for new Nodes.
        private const int NodeCount = 0;

        // TODO: use TimeFrame
        private long _beginTime = -1;
        private long _endTime = -1;
        private HashSet<Node> _descendants;


        /**
        /// Create a new Node
         *
        /// @param word      the word of this node
        /// @param beginTime the start time of the word
        /// @param endTime   the end time of the word
         */
        public Node(Word word, int beginTime, int endTime) 
            :this(GetNextNodeId(), word, beginTime, endTime)
        {
        }


        /**
        /// Create a new Node with given ID. Used when creating a Lattice from a .LAT file
         *
        /// @param id
        /// @param word
        /// @param beginTime
        /// @param endTime
         */
        public Node(String id, Word word, long beginTime, long endTime) 
        {
            LeavingEdges = new List<Edge>();
            EnteringEdges = new List<Edge>();
            this.Id = id;
            this.Word = word;
            this._beginTime = beginTime;
            this._endTime = endTime;
            if (endTime != -1) {
                if (beginTime > endTime) {
                    throw new Exception("Begin time (" + beginTime +
                            ") later than end time (" + endTime + ')');
                }
            }
            ForwardScore = LogMath.LogZero;
            BackwardScore = LogMath.LogZero;
            Posterior = LogMath.LogZero;
        }


        /**
        /// Get a unique ID for a new Node. Used when creating a Lattice from a .LAT file
         *
        /// @return the unique ID for a new node
         */
        protected static string GetNextNodeId() 
        {
            return NodeCount.ToString(CultureInfo.InvariantCulture);
        }


        /**
        /// Test if a node has an Edge to a Node
         *
        /// @param n
        /// @return unique Node ID
         */
        public Boolean HasEdgeToNode(Node n) 
        {
            return GetEdgeToNode(n) != null;
        }


        /**
        /// given a node find the edge to that node
         *
        /// @param n the node of interest
        /// @return the edge to that node or <code> null</code>  if no edge could be found.
         */
        public Edge GetEdgeToNode(Node n) 
        {
            foreach (Edge e in LeavingEdges) 
            {
                if (e.ToNode == n) 
                {
                    return e;
                }
            }
            return null;
        }


        /**
        /// Test is a Node has an Edge from a Node
         *
        /// @param n
        /// @return true if this node has an Edge from n
         */
        public Boolean HasEdgeFromNode(Node n) 
        {
            return GetEdgeFromNode(n) != null;
        }


        /**
        /// given a node find the edge from that node
         *
        /// @param n the node of interest
        /// @return the edge from that node or <code> null</code>  if no edge could be found.
         */
        public Edge GetEdgeFromNode(Node n) 
        {
            foreach (Edge e in EnteringEdges) 
            {
                if (e.FromNode == n) 
                {
                    return e;
                }
            }
            return null;
        }


        /**
        /// Test if a Node has all Edges from the same Nodes and another Node.
         *
        /// @param n
        /// @return true if this Node has Edges from the same Nodes as n
         */
        public  Boolean HasEquivalentEnteringEdges(Node n) 
        {
            if (EnteringEdges.Count != n.EnteringEdges.Count) 
            {
                return false;
            }
            foreach (Edge e in EnteringEdges) 
            {
                Node fromNode = e.FromNode;
                if (!n.HasEdgeFromNode(fromNode)) 
                {
                    return false;
                }
            }
            return true;
        }


        /**
        /// Test if a Node has all Edges to the same Nodes and another Node.
         *
        /// @param n the node of interest
        /// @return true if this Node has all Edges to the sames Nodes as n
         */
        public Boolean HasEquivalentLeavingEdges(Node n) 
        {
            if (LeavingEdges.Count != n.LeavingEdges.Count) 
            {
                return false;
            }
            foreach (Edge e in LeavingEdges) 
            {
                Node toNode = e.ToNode;
                if (!n.HasEdgeToNode(toNode)) 
                {
                    return false;
                }
            }
            return true;
        }


        /**
        /// Get the Edges to this Node
         *
        /// @return Edges to this Node
         */

        public List<Edge> EnteringEdges { get; private set; }


        /**
        /// Get the Edges from this Node
         *
        /// @return Edges from this Node
         */

        public List<Edge> LeavingEdges { get; private set; }

        /**
        /// Returns a copy of the Edges to this Node, so that the underlying data structure will not be modified.
         *
        /// @return a copy of the edges to this node
         */
        public List<Edge> GetCopyOfEnteringEdges() 
        {
            return new List<Edge>(EnteringEdges);
        }

        /**
        /// Returns a copy of the Edges from this Node, so that the underlying data structure will not be modified.
         *
        /// @return a copy of the edges from this node
         */
        public List<Edge> GetCopyOfLeavingEdges() 
        {
            return new List<Edge>(LeavingEdges);
        }

        /**
        /// Add an Edge from this Node
         *
        /// @param e
         */
        public void AddEnteringEdge(Edge e) 
        {
            EnteringEdges.Add(e);
        }


        /**
        /// Add an Edge to this Node
         *
        /// @param e
         */
        public void AddLeavingEdge(Edge e) 
        {
            LeavingEdges.Add(e);
        }


        /**
        /// Remove an Edge from this Node
         *
        /// @param e
         */
        public void RemoveEnteringEdge(Edge e) 
        {
            EnteringEdges.Remove(e);
        }


        /**
        /// Remove an Edge to this Node
         *
        /// @param e the edge to remove
         */
        public void RemoveLeavingEdge(Edge e) 
        {
            LeavingEdges.Remove(e);
        }


        /**
        /// Get the ID associated with this Node
         *
        /// @return the ID
         */

        public string Id { get; private set; }


        /**
        /// Get the word associated with this Node
         *
        /// @return the word
         */

        public Word Word { get; private set; }


        /**
        /// Get the frame number when the word began
         *
        /// @return the begin frame number, or -1 if the frame number is unknown
         */

        public long BeginTime
        {
            get
            {
                if (_beginTime == -1)
                {
                    CalculateBeginTime();
                }
                return _beginTime;
            }

            set
            {
                if (value > EndTime)
                {
                    throw new Exception("Attempting to set a begin time (" + value +
                                        ") that is later than the end time (" +
                                        EndTime + ").");
                }
                this._beginTime = value;
            }
        }



        /**
        /// Get the frame number when the word ends
         *
        /// @return the end time, or -1 if the frame number if is unknown
         */

        public long EndTime
        {
            get { return _endTime; }
            set
            {
                if (BeginTime > value)
                {
                    throw new Exception("Attempting to set an end time (" + value +
                                        ") that is earlier than the start time (" +
                                        BeginTime + ").");
                }
                this._endTime = value;
            }
        }


        /**
        /// Sets the frame number when the words ended. The end time must not be earlier than the time returned by the
        /// getEndTime() method, otherwise an error will be thrown.
         *
        /// @param endTime the frame number when the word ended
         */


        /**
        /// Returns a description of this Node that contains the word, the start time, and the end time.
         *
        /// @return a description of this Node
         */

        public override string ToString() 
        {
            return ("Node(" + Word.Spelling + "," + BeginTime + "|"+
                    EndTime + ')');
        }


        /**
        /// Internal routine when dumping Lattices as AiSee files
         *
        /// @param f
        /// @throws IOException
         */
        public void DumpAISee(StreamWriter f)
        {
            string posterior = Posterior.ToString(CultureInfo.InvariantCulture);
            if (Posterior == LogMath.LogZero) 
            {
                posterior = "log zero";
            }
            f.Write("node: { title: \"" + Id + "\" label: \""
                    + Word + '[' + BeginTime + ',' + EndTime +
                    " p:" + posterior + "]\" }\n");
        }

        /**
        /// Internal routine when dumping Lattices as Graphviz files
        /// 
        /// @param f
        /// @throws IOException
         */
        public void DumpDot(StreamWriter f)
        {
            string posterior = Posterior.ToString(CultureInfo.InvariantCulture);
            if (Posterior == LogMath.LogZero) 
            {
                posterior = "log zero";
            }
            string label = Word.ToString() + '[' + BeginTime + ',' + EndTime + " p:" + posterior + ']';
            f.Write("\tnode" + Id + " [ label=\"" + label + "\" ]\n");
        }

        /**
        /// Internal routine used when dumping Lattices as .LAT files
         *
        /// @param f
        /// @throws IOException
         */
        public void Dump(StreamWriter f)
        {
            f.WriteLine("node: " + Id + ' ' + Word.Spelling +
                    //" a:" + getForwardProb() + " b:" + getBackwardProb()
                    " p:" + Posterior);
        }


        /**
        /// Internal routine used when loading Lattices from .LAT files
         *
        /// @param lattice
        /// @param tokens
         */
        public static void Load(Lattice lattice, StringTokenizer tokens) {

            string id = tokens.nextToken();
            string label = tokens.nextToken();

            lattice.AddNode(id, label, 0, 0);
        }


        /**
        /// Returns the backward score, which is calculated during the computation of the posterior score for this node.
         *
        /// @return Returns the backwardScore.
         */

        public double BackwardScore { get; set; }


        /**
        /// Sets the backward score for this node.
         *
        /// @param backwardScore The backwardScore to set.
         */


        /**
        /// Returns the forward score, which is calculated during the computation of the posterior score for this node.
         *
        /// @return Returns the forwardScore.
         */

        public double ForwardScore { get; set; }


        /**
        /// Sets the backward score for this node.
         *
        /// @param forwardScore The forwardScore to set.
         */


        /**
        /// Returns the posterior probability of this node. Refer to the javadocs for this class for a description of
        /// posterior probabilities.
         *
        /// @return Returns the posterior probability of this node.
         */

        public double Posterior { get; set; }


        /**
        /// Sets the posterior probability of this node. Refer to the javadocs for this class for a description of posterior
        /// probabilities.
         *
        /// @param posterior The node posterior probability to set.
         */


        /** @see java.lang.Object#hashCode() */

        public override int GetHashCode() 
        {
            return Id.GetHashCode();
        }


        /**
        /// Assumes ids are unique node identifiers
         *
        /// @see java.lang.Object#equals(java.lang.Object)
         */

        public override bool Equals(Object obj) 
        {
            return obj is Node && Id.Equals(((Node) obj).Id);
        }


        /**
        /// Calculates the begin time of this node, in the event that the begin time was not specified. The begin time is the
        /// latest of the end times of its predecessor nodes.
         */
        private void CalculateBeginTime() 
        {
            _beginTime = 0;
            foreach (Edge edge in EnteringEdges) 
            {
                if (edge.FromNode.EndTime > _beginTime) 
                {
                    _beginTime = edge.FromNode.EndTime;
                }
            }
        }


        /**
        /// Get the nodes at the other ends of outgoing edges of this node.
         *
        /// @return a list of child nodes
         */
        public List<Node> GetChildNodes() 
        {
            List<Node> childNodes = new List<Node>();
            foreach (Edge edge in LeavingEdges) 
            {
                childNodes.Add(edge.ToNode);
            }
            return childNodes;
        }


        protected internal void CacheDescendants() 
        {
            _descendants = new HashSet<Node>();
            CacheDescendantsHelper(this);
        }


        protected void CacheDescendantsHelper(Node n) 
        {
            foreach (Node child in n.GetChildNodes()) 
            {
                if (_descendants.Contains(child)) 
                {
                    continue;
                }
                _descendants.Add(child);
                CacheDescendantsHelper(child);
            }
        }


        protected Boolean IsAncestorHelper(List<Node> children, Node node, HashSet<Node> seenNodes) 
        {
            foreach (Node n in children) 
            {
                if (seenNodes.Contains(n)) 
                {
                    continue;
                }
                seenNodes.Add(n);
                if (n.Equals(node)) 
                {
                    return true;
                }
                if (IsAncestorHelper(n.GetChildNodes(), node, seenNodes)) 
                {
                    return true;
                }
            }
            return false;
        }


        /**
        /// Check whether this node is an ancestor of another node.
         *
        /// @param node the Node to check
        /// @return whether this node is an ancestor of the passed in node.
         */
        public Boolean IsAncestorOf(Node node) 
        {
            if (_descendants != null) 
            {
                return _descendants.Contains(node);
            }
            if (Equals(node)) 
            {
                return true; // node is its own ancestor
            }
            var seenNodes = new HashSet<Node>();
            seenNodes.Add(this);
            return IsAncestorHelper(GetChildNodes(), node, seenNodes);
        }


        /**
        /// Check whether this node has an ancestral relationship with another node (i.e. either this node is an ancestor of
        /// the other node, or vice versa)
         *
        /// @param node the Node to check for a relationship
        /// @return whether a relationship exists
         */
        public Boolean HasAncestralRelationship(Node node) 
        {
            return IsAncestorOf(node) || node.IsAncestorOf(this);
        }


        /**
        /// Returns true if the given node is equivalent to this node. Two nodes are equivalent only if they have the same
        /// word, the same number of entering and leaving edges, and that their begin and end times are the same.
         *
        /// @param other the Node we're comparing to
        /// @return true if the Node is equivalent; false otherwise
         */
        public Boolean IsEquivalent(Node other) 
        {
            return
                    ((Word.Spelling.Equals(other.Word.Spelling) &&
                            (EnteringEdges.Count == other.EnteringEdges.Count &&
                                    LeavingEdges.Count == other.LeavingEdges.Count)) &&
                            (BeginTime == other.BeginTime &&
                                    _endTime == other.EndTime));
        }


        /**
        /// Returns a leaving edge that is equivalent to the given edge. Two edges are eqivalent if Edge.isEquivalent()
        /// returns true.
         *
        /// @param edge the Edge to compare the leaving edges of this node against
        /// @return an equivalent edge, if any; or null if no equivalent edge
         */
        public Edge FindEquivalentLeavingEdge(Edge edge) 
        {
            foreach (Edge e in LeavingEdges) 
            {
                if (e.IsEquivalent(edge)) 
                {
                    return e;
                }
            }
            return null;
        }


        /**
        /// Returns the best predecessor for this node.
         *
        /// @return Returns the bestPredecessor.
         */

        public Node BestPredecessor { get; set; }


        /**
        /// Sets the best predecessor of this node.
         *
        /// @param bestPredecessor The bestPredecessor to set.
         */


        /**
        /// Returns the Viterbi score for this node. The Viterbi score is usually computed during the speech recognition
        /// process.
         *
        /// @return Returns the viterbiScore.
         */

        public double ViterbiScore { get; set; }
    }
}
