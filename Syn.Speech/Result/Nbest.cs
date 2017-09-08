using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Syn.Speech.Result
{
    /// <summary>
    /// NBest list with A*
    /// </summary>
    public class Nbest
    {
         protected Lattice lattice;

        public Nbest(Lattice lattice) 
        {
            this.lattice = lattice;
        }

        
        public List<String> getNbest(int n) 
        {
            lattice.computeNodePosteriors(1.0f);
            HashSet<String> result = new HashSet<String>();
            BoundedPriorityQueue<NBestPath> queue =
                new BoundedPriorityQueue<NBestPath>(n);

            queue.add(new NBestPath("<s>", lattice.getInitialNode(), 0, 0));

            while (result.Count < n && queue.size() > 0) 
            {
                NBestPath path = queue.poll();
                if (path.node.Equals(lattice.terminalNode)) {
                    result.Add(path.path);
                    continue;
                }

                foreach (Edge e in path.node.getLeavingEdges()) 
                {
                    Node newNode = e.getToNode();
                
                    double newForwardScore = path.forwardScore
                            + e.getAcousticScore() + e.getLMScore();

                    double newScore = newForwardScore + newNode.getBackwardScore();

                    String newPathString = getNewPathString(path, newNode);
                
                    NBestPath newPath = new NBestPath(newPathString, newNode, newScore, newForwardScore);
                
                    queue.add(newPath);
                }
                // printQueue(queue);
            }

            return result.ToList();
        }

        private String getNewPathString(NBestPath path, Node newNode) {
            String newPathString;
            if (newNode.getWord().isSentenceEndWord())
                newPathString = path.path + " </s>";
            else if (newNode.getWord().isFiller())
                newPathString = path.path;
            else
                newPathString = path.path + " " + newNode.getWord();
            return newPathString;
        }

        
        private void printQueue(BoundedPriorityQueue<NBestPath> queue) 
        {
            Trace.WriteLine("");
            foreach (NBestPath p in queue) 
            {
               Trace.WriteLine(p);
            }
        }

    }

    class NBestPath:IComparable<NBestPath> 
    {
        public String path;
        public Node node;
        double score;
        public double forwardScore;

        public NBestPath(String path, Node node, double score,
                double forwardScore) 
            :base()
        {
            
            this.path = path;
            this.node = node;
            this.score = score;
            this.forwardScore = forwardScore;
        }

        public int compareTo(NBestPath o) 
        {
            return score.CompareTo(o.score);
        }

        override
        public String ToString() {
            return path + " [" + score + ',' + forwardScore + ']';
        }
    
        int IComparable<NBestPath>.CompareTo(NBestPath other)
        {
 	        return compareTo(other);
        }
}

}
