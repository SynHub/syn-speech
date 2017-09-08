using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Results
{
   
/**
 * Parent to all sausage makers.
 *
 * @author P. Gorniak
 */
public abstract class AbstractSausageMaker : IConfidenceScorer, IConfigurable {

    /**
     * A Cluster is a set of Nodes together with their earliest start time and latest end time. A SausageMaker builds up
     * a sequence of such clusters that then gets turned into a Sausage.
     *
     * @see Node
     * @see Sausage
     * @see SausageMaker
     */

    public class Cluster : IEnumerable<Node> {

        public int startTime;
        public int endTime;
        internal LinkedList<Node> elements = new LinkedList<Node>();


        public Cluster(Node n) {
            startTime = n.getBeginTime();
            endTime = n.getEndTime();
            Java.Add(elements,n);
        }


        public Cluster(int start, int end) {
            startTime = start;
            endTime = end;
        }


        public void add(Node n) {
            if (n.getBeginTime() < startTime) {
                startTime = n.getBeginTime();
            }
            if (n.getEndTime() > endTime) {
                endTime = n.getEndTime();
            }
            Java.Add(elements, n);
        }


        public void add(Cluster c) {
            if (c.startTime < startTime) {
                startTime = c.startTime;
            }
            if (c.endTime > endTime) {
                endTime = c.endTime;
            }
            Java.AddAll(elements,c.getElements());
        }

        public IEnumerator<Node> GetEnumerator() {
            return elements.GetEnumerator();
        }


       
        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("s: ").Append(startTime).Append(" e: ").Append(endTime).Append('[');
            foreach (Node node in elements)
                sb.Append(node).Append(',');
            if (!elements.IsEmpty())
                sb.Length= (sb.Length - 1);
            sb.Append(']');
            return sb.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /** @return Returns the elements. */
        public LinkedList<Node> getElements() {
            return elements;
        }


        /** @param elements The elements to set. */
        public void setElements(LinkedList<Node> elements) {
            this.elements = elements;
        }
    }

    internal class ClusterComparator : IComparer<Cluster> {

        /**
         * Compares to clusters according to their topological relationship. Relies on strong assumptions about the
         * possible constituents of clusters which will only be valid during the sausage creation process.
         *
         * @param cluster1 the first cluster
         * @param cluster2 the second cluster
         */
        public int Compare(Cluster cluster1, Cluster cluster2) {
            foreach (Node n1 in cluster1) {
                foreach (Node n2 in cluster2) {
                    if (n1.isAncestorOf(n2)) {
                        return -1;
                    } else if (n2.isAncestorOf(n1)) {
                        return 1;
                    }
                }
            }
            return 0;
        }
    }

    /** The property that defines the language model weight. */
    [S4Double(defaultValue = 1.0)]
    public const String PROP_LANGUAGE_WEIGHT = "languageWeight";

    protected float languageWeight;

    protected Lattice lattice;


    public AbstractSausageMaker() {
    }
   
    /** @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet) */
    public void newProperties(PropertySheet ps)  {
        languageWeight = ps.getFloat(PROP_LANGUAGE_WEIGHT);
    }

    public abstract IConfidenceResult score(Result result);


    protected static int getOverlap(Node n, int startTime, int endTime) {
        return Math.Min(n.getEndTime(), endTime) -
                Math.Max(n.getBeginTime(), startTime);
    }


    protected static int getOverlap(Node n1, Node n2) {
        return Math.Min(n1.getEndTime(), n2.getEndTime()) -
                Math.Max(n1.getBeginTime(), n2.getBeginTime());
    }


    /**
     * Returns true if the two given clusters has time overlaps.
     *
     * @param cluster1 the first cluster to examine
     * @param cluster2 the second cluster to examine
     * @return true if the clusters has overlap, false if they don't
     */
    protected bool hasOverlap(Cluster cluster1, Cluster cluster2) {
        return (cluster1.startTime < cluster2.endTime &&
                cluster2.startTime < cluster1.endTime);
    }


    /**
     * Return the total probability mass of the subcluster of nodes of the given cluster that all have the given word as
     * their word.
     *
     * @param cluster the cluster to subcluster from
     * @param word    the word to subcluster by
     * @return the log probability mass of the subcluster formed by the word
     */
    protected double wordSubClusterProbability(LinkedList<Node> cluster, String word) {
        return clusterProbability(makeWordSubCluster(cluster, word));
    }


    /**
     * Return the total probability mass of the subcluster of nodes of the given cluster that all have the given word as
     * their word.
     *
     * @param cluster the cluster to subcluster from
     * @param word    the word to subcluster by
     * @return the log probability mass of the subcluster formed by the word
     */
    protected double wordSubClusterProbability(Cluster cluster, String word) {
        return clusterProbability(makeWordSubCluster(cluster, word));
    }


    /**
     * Calculate the sum of posteriors in this cluster.
     *
     * @param cluster the cluster to sum over
     * @return the probability sum
     */
    protected double clusterProbability(LinkedList<Node> cluster) {
        float p = LogMath.LOG_ZERO;
        LogMath logMath = LogMath.getLogMath();

        foreach (Node node in cluster)
            p = logMath.addAsLinear(p, (float)node.getPosterior());

        return p;
    }


    /**
     * Calculate the sum of posteriors in this cluster.
     *
     * @param cluster the cluster to sum over
     * @return the probability sum
     */
    protected double clusterProbability(Cluster cluster) {
        return clusterProbability(cluster.elements);
    }


    /**
     * Form a subcluster by extracting all nodes corresponding to a given word.
     *
     * @param cluster the parent cluster
     * @param word    the word to cluster by
     * @return the subcluster.
     */
    protected LinkedList<Node> makeWordSubCluster(LinkedList<Node> cluster, String word)
    {
        var sub = new LinkedList<Node>();
        foreach (Node n in cluster) {
            if (n.getWord().getSpelling().Equals(word)) {
               Java.Add(sub,n);
            }
        }
        return sub;
    }


    /**
     * Form a subcluster by extracting all nodes corresponding to a given word.
     *
     * @param cluster the parent cluster
     * @param word    the word to cluster by
     * @return the subcluster.
     */
    protected Cluster makeWordSubCluster(Cluster cluster, String word) {
        var l = makeWordSubCluster(cluster.elements, word);
        Cluster c = new Cluster(cluster.startTime, cluster.endTime);
        c.elements = l;
        return c;
    }


    /**
     * print out a list of clusters for debugging
     *
     * @param clusters
     */
    protected void printClusters(List<Cluster> clusters)
    {
        var i = clusters.GetEnumerator();
        int j = 0;
        while (i.MoveNext())
        {
            j++;
            Console.WriteLine("----cluster " + j + " : ");
            Console.WriteLine(i.Current);
        }
        Console.WriteLine("----");
    }


    /**
     * Turn a list of lattice node clusters into a Sausage object.
     *
     * @param clusters the list of node clusters in topologically correct order
     * @return the Sausage corresponding to the cluster list
     */
    protected Sausage sausageFromClusters(List<Cluster> clusters) {
        Sausage sausage = new Sausage(clusters.Count);
        int index = 0;
        foreach (Cluster cluster in clusters) {
            HashSet<String> seenWords = new HashSet<String>();
            foreach (Node node in cluster) {
                Word word = node.getWord();
                if (seenWords.Contains(word.getSpelling())) {
                    continue;
                }
                seenWords.Add(word.getSpelling());
                WordResult swr =
                    new WordResult(
                            node,
                            wordSubClusterProbability(
                                cluster, word.getSpelling()));
                sausage.addWordHypothesis(index, swr);
            }
            index++;
        }
        sausage.fillInBlanks();
        return sausage;
    }
}
}
