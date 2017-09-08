using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//PATROLLED
namespace Syn.Speech.Results
{
    
/**
 * <p/>
 * The SausageMaker takes word lattices as input and turns them into sausages (Confusion Networks) according to Mangu,
 * Brill and Stolcke, "Finding Consensus in Speech Recognition: word error minimization and other applications of
 * confusion networks", Computer Speech and Language, 2000. Note that the <code>getBestHypothesis</code> of the
 * ConfidenceResult object returned by the {@link #score(Result) score} method returns the path where all the words have
 * the highest posterior probability within its corresponding time slot. </p>
 *
 * @author pgorniak
 */
public class SausageMaker : AbstractSausageMaker {

    /** Construct an empty sausage maker */
    public SausageMaker() {
    }
    
    public SausageMaker(float languageWieght) {
        languageWeight = languageWieght;
    }

    /**
     * Construct a sausage maker
     *
     * @param l the lattice to construct a sausage from
     */
    public SausageMaker(Lattice l) {
        lattice = l;
    }


    /**
     * Perform the inter word clustering stage of the algorithm
     *
     * @param clusters the current cluster set
     */
    protected void interWordCluster(List<Cluster> clusters) {
        while (interWordClusterStep(clusters)) ;
    }


    /**
     * Returns the latest begin time of all nodes in the given cluster.
     *
     * @param cluster the cluster to examine
     * @return the latest begin time
     */
    public int getLatestBeginTime(List<Node> cluster) {
        if (cluster.IsEmpty()) {
            return -1;
        }
        int startTime = 0;
        foreach (Node n in cluster) {
            if (n.getBeginTime() > startTime) {
                startTime = n.getBeginTime();
            }
        }
        return startTime;
    }


    /**
     * Returns the earliest end time of all nodes in the given cluster.
     *
     * @param cluster the cluster to examine
     * @return the earliest end time
     */
    public int getEarliestEndTime(List<Node> cluster) {
        if (cluster.Count == 0) {
            return -1;
        }
        int endTime = Integer.MAX_VALUE;
        foreach (Node n in cluster) {
            if (n.getEndTime() < endTime) {
                endTime = n.getEndTime();
            }
        }
        return endTime;
    }


    /**
     * Perform one inter word clustering step of the algorithm
     *
     * @param clusters the current cluster set
     */
    protected bool interWordClusterStep(List<Cluster> clusters) {
        Cluster toBeMerged1 = null;
        Cluster toBeMerged2 = null;
        double maxSim = Double.NegativeInfinity;

        //TODO: Check Behaviour
        for (int i = 0; i < clusters.Count; i++)
        {
            Cluster c1 = clusters[i];
            if (i + 1 >= clusters.Count) break;
            for (int j = i + 1; j < clusters.Count; j++)
            {
                Cluster c2 = clusters[j];
                double sim = interClusterDistance(c1, c2);
                if (sim > maxSim && hasOverlap(c1, c2))
                {
                    maxSim = sim;
                    toBeMerged1 = c1;
                    toBeMerged2 = c2;
                }
            }

        }
        if (toBeMerged1 != null) {
            clusters.Remove(toBeMerged2);
            toBeMerged1.add(toBeMerged2);
            return true;
        }
        return false;
    }


    /**
     * Find the string edit distance between to lists of objects. 
     * Objects are compared using .equals() 
     * TODO: could be moved to a general utility class
     *
     * @param p1 the first list
     * @param p2 the second list
     * @return the string edit distance between the two lists
     */
    protected static int stringEditDistance(IList p1, IList p2) {
        if (p1.Count==0) {
            return p2.Count;
        }
        if (p2.Count==0) {
            return p1.Count;
        }
        int[,] distances = new int[p1.Count + 1,p2.Count + 1];
        for (int i = 0; i <= p1.Count; i++) {
            distances[i,0] = i;
        }
        for (int j = 0; j <= p2.Count; j++) {
            distances[0,j] = j;
        }
        for (int i = 1; i <= p1.Count; i++) {
            for (int j = 1; j <= p2.Count; j++) {
                int min = Math.Min(distances[i - 1,j - 1]
                        + (p1[i - 1].Equals(p2[j - 1]) ? 0 : 1),
                        distances[i - 1,j] + 1);
                min = Math.Min(min, distances[i,j - 1] + 1);
                distances[i,j] = min;
            }
        }
        return distances[p1.Count,p2.Count];
    }


    /**
     * Compute the phonetic similarity of two lattice nodes, based on the string edit distance between their most likely
     * pronunciations. TODO: maybe move to Node.java?
     *
     * @param n1 the first node
     * @param n2 the second node
     * @return the phonetic similarity, between 0 and 1
     */
    protected double computePhoneticSimilarity(Node n1, Node n2) {
        Pronunciation p1 = n1.getWord().getMostLikelyPronunciation();
        Pronunciation p2 = n2.getWord().getMostLikelyPronunciation();
        double sim = stringEditDistance(p1.getUnits().ToList(),p2.getUnits().ToList());
        sim /= (p1.getUnits().Length + p2.getUnits().Length);
        return 1 - sim;
    }


    /**
     * Calculate the distance between two clusters
     *
     * @param c1 the first cluster
     * @param c2 the second cluster
     * @return the inter cluster similarity, or Double.NEGATIVE_INFINITY if these clusters should never be clustered
     *         together.
     */
    protected double interClusterDistance(Cluster c1, Cluster c2) {
        if (areClustersInRelation(c1, c2)) {
            return Double.NegativeInfinity;
        }
        float totalSim = LogMath.LOG_ZERO;
        float wordPairCount = (float) 0.0;
        HashSet<String> wordsSeen1 = new HashSet<String>();
        LogMath logMath = LogMath.getLogMath();

        foreach (Node node1 in c1.getElements()) {
            String word1 = node1.getWord().getSpelling();
            if (wordsSeen1.Contains(word1)) {
                continue;
            }
            wordsSeen1.Add(word1);
            HashSet<String> wordsSeen2 = new HashSet<String>();
            foreach (Node node2 in c2.getElements()) {
                String word2 = node2.getWord().getSpelling();
                if (wordsSeen2.Contains(word2)) {
                    continue;
                }
                wordsSeen2.Add(word2);
                float sim = (float) computePhoneticSimilarity(node1, node2);
                sim = logMath.linearToLog(sim);
                sim += (float)wordSubClusterProbability(c1, word1);
                sim += (float)wordSubClusterProbability(c2, word2);
                totalSim = logMath.addAsLinear(totalSim, sim);
                wordPairCount++;
            }
        }
        return totalSim - logMath.logToLinear(wordPairCount);
    }


    /**
     * Check whether these to clusters stand in a relation to each other. Two clusters are related if a member of one is
     * an ancestor of a member of the other cluster.
     *
     * @param cluster1 the first cluster
     * @param cluster2 the second cluster
     * @return true if the clusters are related
     */
    protected bool areClustersInRelation(Cluster cluster1, Cluster cluster2) {
        foreach (Node n1 in cluster1.getElements()) {
            foreach (Node n2 in cluster2.getElements()) {
                if (n1.hasAncestralRelationship(n2)) {
                    return true;
                }
            }
        }
        return false;
    }


    /**
     * Calculate the distance between two clusters, forcing them to have the same words in them, and to not be related
     * to each other.
     *
     * @param cluster1 the first cluster
     * @param cluster2 the second cluster
     * @return The intra-cluster distance, or Double.NEGATIVE_INFINITY if the clusters should never be clustered
     *         together.
     */
    protected double intraClusterDistance(Cluster cluster1, Cluster cluster2) {
        LogMath logMath = LogMath.getLogMath();
        double maxSim = Double.NegativeInfinity;

        foreach (Node node1 in cluster1.getElements()) {
            foreach (Node node2 in cluster2.getElements()) {
                if (!node1.getWord().getSpelling().Equals(
                            node2.getWord().getSpelling()))
                    return Double.NegativeInfinity;

                if (node1.hasAncestralRelationship(node2))
                    return Double.NegativeInfinity;

                double overlap = getOverlap(node1, node2);
                if (overlap > 0.0) {
                    overlap = logMath.logToLinear((float) overlap);
                    overlap += node1.getPosterior() + node2.getPosterior();
                    if (overlap > maxSim) {
                        maxSim = overlap;
                    }
                }
            }
        }
        return maxSim;
    }


    /**
     * Perform the intra word clustering stage of the algorithm
     *
     * @param clusters the current list of clusters
     */
    protected void intraWordCluster(List<Cluster> clusters) {
        while (intraWordClusterStep(clusters)) ;
    }


    /**
     * Perform a step of the intra word clustering stage
     *
     * @param clusters the current list of clusters
     * @return did two clusters get merged?
     */
    protected bool intraWordClusterStep(List<Cluster> clusters) {
        Cluster toBeMerged1 = null;
        Cluster toBeMerged2 = null;
        double maxSim = Double.NegativeInfinity;

        //TODO: Check Behaviour
        for (int i = 0; i < clusters.Count;i++)
        {
            Cluster c1 = clusters[i];
            if (i + 1 >= clusters.Count) break;

            for (int j = i + 1; j < clusters.Count; j++)
            {
                Cluster c2 = clusters[j];
                double sim = intraClusterDistance(c1, c2);
                if (sim > maxSim)
                {
                    maxSim = sim;
                    toBeMerged1 = c1;
                    toBeMerged2 = c2;
                }
            }
        }
        if (toBeMerged1 != null) {
            clusters.Remove(toBeMerged2);
            toBeMerged1.add(toBeMerged2);
            return true;
        }
        return false;
    }


    /**
     * Turn the lattice contained in this sausage maker into a sausage object.
     *
     * @return the sausage producing by collapsing the lattice.
     */
    public Sausage makeSausage() {
        List<Cluster> clusters = new List<Cluster>(lattice.nodes.size());
        foreach (Node n in lattice.nodes.values()) {
            n.cacheDescendants();
            Cluster bucket = new Cluster(n);
            clusters.Add(bucket);
        }
        intraWordCluster(clusters);
        interWordCluster(clusters);
        clusters = topologicalSort(clusters);
        return sausageFromClusters(clusters);
    }


    /** @see edu.cmu.sphinx.result.ConfidenceScorer#score(edu.cmu.sphinx.result.Result) */
    public override IConfidenceResult score(Result result) {
        lattice = new Lattice(result);
        LatticeOptimizer lop = new LatticeOptimizer(lattice);
        lop.optimize();
        lattice.computeNodePosteriors(languageWeight);
        return makeSausage();
    }


    /**
     * Topologically sort the clusters. Note that this is a brute force sort by removing the min cluster from the list
     * of clusters, since Collections.sort() does not work in all cases.
     *
     * @param clusters the list of clusters to be topologically sorted
     * @return a topologically sorted list of clusters
     */
    private List<Cluster> topologicalSort(List<Cluster> clusters) {
        var comparator = new ClusterComparator();
        List<Cluster> sorted = new List<Cluster>(clusters.Count);
        while (!clusters.IsEmpty())
        {
            Cluster cluster = Java.Min(clusters, comparator);
            clusters.Remove(cluster);
            sorted.Add(cluster);
        }
        return sorted;
    }
}

}
