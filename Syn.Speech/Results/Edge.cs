using System;
using System.Globalization;
using System.IO;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    /// Edges are part of Lattices.  They connect Nodes, and contain the score associated with that sequence.
    /// </summary>
    public class Edge
    {
        /**
        /// Create an Edge from fromNode to toNode with acoustic and Language Model scores.
         *
        /// @param fromNode
        /// @param toNode
        /// @param acousticScore
        /// @param lmScore
         */
        public Edge(Node fromNode, Node toNode, double acousticScore, double lmScore) 
        {
            this.AcousticScore = acousticScore;
            this.LMScore = lmScore;
            this.FromNode = fromNode;
            this.ToNode = toNode;
        }

        public override string ToString() 
        {
            return "Edge(" + FromNode + "-->" + ToNode + '[' + AcousticScore
                    + ',' + LMScore + "])";
        }

        /**
        /// Internal routine used when creating a Lattice from a .LAT file
         *
        /// @param lattice
        /// @param tokens
         */
        public static void Load(Lattice lattice, StringTokenizer tokens) 
        {

            string from = tokens.nextToken();
            string to = tokens.nextToken();
            int score = int.Parse(tokens.nextToken(), CultureInfo.InvariantCulture.NumberFormat);

            Node fromNode = lattice.GetNode(from);
            if (fromNode == null) {
                throw new Exception("Edge fromNode \"" + from + "\" does not exist");
            }

            Node toNode = lattice.GetNode(to);
            if (toNode == null) {
                throw new Exception("Edge toNode \"" + to + "\" does not exist");
            }

            lattice.AddEdge(fromNode, toNode, score, 0.0);
        }

        /**
        /// Internal routine used when dumping a Lattice as a .LAT file
         *
        /// @param f
        /// @throws IOException
         */
        public void Dump(StreamWriter f)
        {
            f.WriteLine("edge: " + FromNode.Id + " " + ToNode.Id + " "
                    + AcousticScore + " " + LMScore);
        }

        /**
        /// Internal routine used when dumping a Lattice as an AiSee file
         *
        /// @param f
        /// @throws IOException
         */
        public void DumpAISee(StreamWriter f)
        {
            f.Write("edge: { sourcename: \"" + FromNode.Id
                    + "\" targetname: \"" + ToNode.Id
                    + "\" label: \"" + AcousticScore + ',' + LMScore + "\" }\n");
        }

        /**
        /// Internal routine used when dumping a Lattice as an Graphviz file
         *
        /// @param f
        /// @throws IOException
         */
        public void DumpDot(StreamWriter f)
        {
            string label = "" + AcousticScore + "," + LMScore;
            f.Write("\tnode" + FromNode.Id + " -> node" + ToNode.Id 
                    + " [ label=\"" + label + "\" ]\n");
        }

        /**
        /// Get the acoustic score associated with an Edge. This is the acoustic
        /// score of the word that this edge is transitioning to, that is, the word
        /// represented by the node returned by the getToNode() method.
        /// 
        /// @return the acoustic score of the word this edge is transitioning to
         */

        public double AcousticScore { get;  set; }

        /**
        /// Get the language model score associated with an Edge
         *
        /// @return the score
         */
        public double LMScore { get; set; }

        /**
        /// Get the "from" Node associated with an Edge
         *
        /// @return the Node
         */
        public Node FromNode { get; protected set; }

        /**
        /// Get the "to" Node associated with an Edge
         *
        /// @return the Node
         */
        public Node ToNode { get; protected set; }

        /**
        /// Returns true if the given edge is equivalent to this edge. Two edges are equivalent only if they have their
        /// 'fromNode' and 'toNode' are equivalent, and that their acoustic and language scores are the same.
         *
        /// @param other the Edge to compare this Edge against
        /// @return true if the Edges are equivalent; false otherwise
         */
        public Boolean IsEquivalent(Edge other) 
        {
            /*
            /// TODO: Figure out why there would be minute differences
            /// in the acousticScore. Therefore, the equality of the acoustic
            /// score is judge based on whether the difference is bigger than 1.
             */
            double diff = Math.Abs(AcousticScore)* 0.00001;
            return ((Math.Abs(AcousticScore - other.AcousticScore) <= diff &&
                    LMScore == other.LMScore) &&
                    (FromNode.IsEquivalent(other.FromNode) &&
                            ToNode.IsEquivalent(other.ToNode)));
        }
    }
}
