using System;
using System.Collections.Generic;
using Syn.Speech.Logging;
//REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    /// NBest list with A*
    /// </summary>
    public class Nbest
    {
        protected Lattice Lattice;

        public Nbest(Lattice lattice)
        {
            Lattice = lattice;
        }


        public ICollection<String> GetNbest(int n)
        {
            Lattice.ComputeNodePosteriors(1.0f);
            HashSet<String> result = new HashSet<String>();
            var queue = new BoundedPriorityQueue<NBestPath>(n);

            queue.Add(new NBestPath("<s>", Lattice.InitialNode, 0, 0));

            while (result.Count < n && queue.Size() > 0)
            {
                NBestPath path = queue.Poll();
                if (path.Node.Equals(Lattice.TerminalNode))
                {
                    result.Add(path.Path);
                    continue;
                }

                foreach (Edge e in path.Node.LeavingEdges)
                {
                    Node newNode = e.ToNode;

                    double newForwardScore = path.ForwardScore
                            + e.AcousticScore + e.LMScore;

                    double newScore = newForwardScore + newNode.BackwardScore;

                    string newPathString = GetNewPathString(path, newNode);

                    NBestPath newPath = new NBestPath(newPathString, newNode, newScore, newForwardScore);

                    queue.Add(newPath);
                }
                // printQueue(queue);
            }

            return result;
        }

        private static string GetNewPathString(NBestPath path, Node newNode)
        {
            string newPathString;
            if (newNode.Word.IsSentenceEndWord)
                newPathString = path.Path + " </s>";
            else if (newNode.Word.IsFiller)
                newPathString = path.Path;
            else
                newPathString = path.Path + " " + newNode.Word;
            return newPathString;
        }


        private void PrintQueue(BoundedPriorityQueue<NBestPath> queue)
        {
            this.LogInfo("");
            foreach (NBestPath p in queue)
            {
                this.LogInfo(p);
            }
        }

    }

    class NBestPath : IComparable<NBestPath>
    {
        public string Path;
        public Node Node;
        readonly double _score;
        public double ForwardScore;

        public NBestPath(String path, Node node, double score,double forwardScore) : base()
        {
            Path = path;
            Node = node;
            _score = score;
            ForwardScore = forwardScore;
        }

        public override string ToString()
        {
            return Path + " [" + _score + ',' + ForwardScore + ']';
        }

        int IComparable<NBestPath>.CompareTo(NBestPath other)
        {
            return _score.CompareTo(other._score);
        }
    }

}
