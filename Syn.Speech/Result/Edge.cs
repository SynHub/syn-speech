using System;
using System.IO;

namespace Syn.Speech.Result
{
    /// <summary>
    /// Edges are part of Lattices.  They connect Nodes, and contain the score associated with that sequence.
    /// </summary>
    public class Edge
    {
        protected double acousticScore;
        protected double lmScore;
        protected Node fromNode=null;
        protected Node toNode=null;


        /**
        /// Create an Edge from fromNode to toNode with acoustic and Language Model scores.
         *
        /// @param fromNode
        /// @param toNode
        /// @param acousticScore
        /// @param lmScore
         */
        public Edge(Node fromNode, Node toNode,
                       double acousticScore, double lmScore) 
        {
            this.acousticScore = acousticScore;
            this.lmScore = lmScore;
            this.fromNode = fromNode;
            this.toNode = toNode;
        }


        override
        public String ToString() 
        {
            return "Edge(" + fromNode + "-->" + toNode + '[' + acousticScore
                    + ',' + lmScore + "])";
        }


        /**
        /// Internal routine used when creating a Lattice from a .LAT file
         *
        /// @param lattice
        /// @param tokens
         */
        public static void load(Lattice lattice, StringTokenizer tokens) 
        {

            String from = tokens.nextToken();
            String to = tokens.nextToken();
            int score = int.Parse(tokens.nextToken());

            Node fromNode = lattice.getNode(from);
            if (fromNode == null) {
                throw new Exception("Edge fromNode \"" + from + "\" does not exist");
            }

            Node toNode = lattice.getNode(to);
            if (toNode == null) {
                throw new Exception("Edge toNode \"" + to + "\" does not exist");
            }

            lattice.addEdge(fromNode, toNode, score, 0.0);
        }


        /**
        /// Internal routine used when dumping a Lattice as a .LAT file
         *
        /// @param f
        /// @throws IOException
         */
        public void dump(StreamWriter f)
        {
            f.WriteLine("edge: " + fromNode.getId() + " " + toNode.getId() + " "
                    + acousticScore + " " + lmScore);
        }


        /**
        /// Internal routine used when dumping a Lattice as an AiSee file
         *
        /// @param f
        /// @throws IOException
         */
        public void dumpAISee(StreamWriter f)
        {
            f.Write("edge: { sourcename: \"" + fromNode.getId()
                    + "\" targetname: \"" + toNode.getId()
                    + "\" label: \"" + acousticScore + ',' + lmScore + "\" }\n");
        }

        /**
        /// Internal routine used when dumping a Lattice as an Graphviz file
         *
        /// @param f
        /// @throws IOException
         */
        public void dumpDot(StreamWriter f)
        {
            String label = "" + acousticScore + "," + lmScore;
            f.Write("\tnode" + fromNode.getId() + " -> node" + toNode.getId() 
                    + " [ label=\"" + label + "\" ]\n");
        }

        /**
        /// Get the acoustic score associated with an Edge. This is the acoustic
        /// score of the word that this edge is transitioning to, that is, the word
        /// represented by the node returned by the getToNode() method.
        /// 
        /// @return the acoustic score of the word this edge is transitioning to
         */
        public double getAcousticScore() 
        {
            return acousticScore;
        }


        /**
        /// Get the language model score associated with an Edge
         *
        /// @return the score
         */
        public double getLMScore() 
        {
            return lmScore;
        }


        /**
        /// Get the "from" Node associated with an Edge
         *
        /// @return the Node
         */
        public Node getFromNode() 
        {
            return fromNode;
        }


        /**
        /// Get the "to" Node associated with an Edge
         *
        /// @return the Node
         */
        public Node getToNode() 
        {
            return toNode;
        }


        /**
        /// Sets the acoustic score
         *
        /// @param v the acoustic score.
         */
        public void setAcousticScore(double v) 
        {
            acousticScore = v;
        }


        /**
        /// Sets the language model score
         *
        /// @param v the lm score.
         */
        public void setLMScore(double v) 
        {
            lmScore = v;
        }


        /**
        /// Returns true if the given edge is equivalent to this edge. Two edges are equivalent only if they have their
        /// 'fromNode' and 'toNode' are equivalent, and that their acoustic and language scores are the same.
         *
        /// @param other the Edge to compare this Edge against
        /// @return true if the Edges are equivalent; false otherwise
         */
        public Boolean isEquivalent(Edge other) 
        {
            /*
            /// TODO: Figure out why there would be minute differences
            /// in the acousticScore. Therefore, the equality of the acoustic
            /// score is judge based on whether the difference is bigger than 1.
             */
            double diff = Math.Abs(acousticScore)* 0.00001;
            return ((Math.Abs(acousticScore - other.getAcousticScore()) <= diff &&
                    lmScore == other.getLMScore()) &&
                    (fromNode.isEquivalent(other.getFromNode()) &&
                            toNode.isEquivalent(other.getToNode())));
        }
    }
}
