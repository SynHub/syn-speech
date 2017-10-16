using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
//REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    /// <summary>
    /// Represents a node in the HMM Tree 

    // For large vocabularies we may create millions of these objects,
    // therefore they are extremely space sensitive. So we want to make
    // these objects as small as possible.  The requirements for these
    // objects when building the tree of nodes are very different from once
    // we have built it. When building, we need to easily add successor
    // nodes and quickly identify duplicate children nodes. After the tree
    // is built we just need to quickly identify successors.  We want the
    // flexibility of a map to manage successors at startup, but we don't
    // want the space penalty (at least 5 32 bit fields per map), instead
    // we'd like an array.  To support this dual mode, we manage the
    // successors in an Object which can either be a Map or a List
    // depending upon whether the node has been frozen or not.
    /// </summary>
    public class Node
    {

        private static int _nodeCount;
        private static int _successorCount;
       
    
        /// <summary>
        /// This can be either Map during tree construction or Array after
        /// tree freeze. Conversion to array helps to save memory
        /// </summary>
        private Object _successors;


        /// <summary>
        /// Creates a node
        /// </summary>
        /// <param name="probability">the unigram probability for the node</param>
        public Node(float probability)
        {
            UnigramProbability = probability;
            _nodeCount++;
            //        if ((nodeCount % 10000) == 0) {
            //             System.out.println("NC " + nodeCount);
            //        }
        }

        /// <summary>
        /// the unigram probability
        /// </summary>
        public float UnigramProbability { get; set; }


        /**
    * Sets the unigram probability
    *
    * @param probability the unigram probability
    */


        /// <summary>
        /// Given an object get the set of successors for this object
        /// </summary>
        /// <param name="key">the object key</param>
        /// <returns>the node containing the successors</returns>
        private Node GetSuccessor(Object key) 
        {
            if (key == null)
                return null;
            Dictionary<Object, Node> successors = GetSuccessorMap();
            if (!successors.ContainsKey(key))
                return null;
            return successors[key];
        }
        

        /// <summary>
        /// Add the child to the set of successors if not exists
        /// </summary>
        /// <param name="key">the object key</param>
        /// <param name="child">the child to add</param>
        public void PutSuccessor(Object key, Node child) 
        {
            Dictionary<Object, Node> successors = GetSuccessorMap();
            if (!successors.ContainsKey(key))
                successors.Add(key, child);
        }

        /// <summary>
        /// Gets the successor map for this node
        /// </summary>
        /// <returns>the successor map</returns>
        public Dictionary<Object, Node> GetSuccessorMap() 
        {
            if (_successors == null) 
            {
                _successors = new Dictionary<Object, Node>();
            }

            Debug.Assert(_successors is Dictionary<Object, Node>);
            return (Dictionary<Object, Node>) _successors;
        }
        

        /// <summary>
        /// Freeze the node. Convert the successor map into an array list
        /// </summary>
        public virtual void Freeze() 
        {
            if (_successors is Dictionary<Object, Node>) 
            {
                Dictionary<Object, Node> map = GetSuccessorMap();
                _successors = map.Values.ToArray();
                foreach (Node node in map.Values) 
                {
                    node.Freeze();
                }
                _successorCount += map.Count;
            }
        }


        static void DumpNodeInfo() 
        {
            Logger.LogInfo<Node>("Nodes: " + _nodeCount + " successors " +
                    _successorCount + " avg " + (_successorCount / _nodeCount));
        }


        /// <summary>
        /// Adds a child node holding an hmm to the successor.  If a node similar to the child has already been added, we use
        /// the previously added node, otherwise we add this. Also, we record the base unit of the child in the set of right
        /// context
        /// </summary>
        /// <param name="hmm">the hmm to add</param>
        /// <param name="probability"></param>
        /// <returns>the node that holds the hmm (new or old)</returns>
        public Node AddSuccessor(IHMM hmm, float probability) 
        {
            if (hmm == null)
                return null;
            Node child = null;
            Node matchingChild = GetSuccessor(hmm);
            if (matchingChild == null) 
            {
                child = new HMMNode(hmm, probability);
                PutSuccessor(hmm, child);
            } 
            else 
            {
                if (matchingChild.UnigramProbability < probability) 
                {
                    matchingChild.UnigramProbability = probability;
                }
                child = matchingChild;
            }
            return child;
        }
      


        /// <summary>
        /// Adds a child node holding a pronunciation to the successor. If a node similar to the child has already been
        /// added, we use the previously added node, otherwise we add this. Also, we record the base unit of the child in the
        /// set of right context
        /// </summary>
        /// <param name="pronunciation">the pronunciation to add</param>
        /// <param name="probability"></param>
        /// <returns>the node that holds the pronunciation (new or old)</returns>
        public WordNode AddSuccessor(Pronunciation pronunciation, float probability, HashMap<Pronunciation, WordNode> wordNodeMap) 
        {
            WordNode child = null;
            WordNode matchingChild = (WordNode)GetSuccessor(pronunciation);
            if (matchingChild == null)
            {
                child = wordNodeMap.Get(pronunciation);
                if (child == null)
                {
                    child = new WordNode(pronunciation, probability);
                    wordNodeMap.Put(pronunciation, child);
                }
                PutSuccessor(pronunciation, child);
            }
            else
            {
                if (matchingChild.UnigramProbability < probability)
                {
                    matchingChild.UnigramProbability = probability;
                }
                child = matchingChild;
            }
            return child;
        }
        

        /// <summary>
        /// add a WordNode succesor
        /// </summary>
        /// <param name="wordNode"></param>
        public void AddSuccessor(WordNode wordNode) 
        {
            PutSuccessor(wordNode, wordNode);
        }


        /// <summary>
        /// Adds an EndNode to the set of successors for this node If a node similar to the child has already been added, we
        /// use the previously added node, otherwise we add this.
        /// </summary>
        /// <param name="child">the endNode to add</param>
        /// <param name="probability"></param>
        /// <returns></returns>
        public EndNode AddSuccessor(EndNode child, float probability) 
        {
            if (child == null)
                return null;
            Unit baseUnit = child.BaseUnit;
            EndNode matchingChild = (EndNode) GetSuccessor(baseUnit);
            if (matchingChild == null) {
                PutSuccessor(baseUnit, child);
            } 
            else 
            {
                if (matchingChild.UnigramProbability < probability) 
                {
                    matchingChild.UnigramProbability = probability;
                }
                child = matchingChild;
            }
            return child;
        }
        

        /// <summary>
        /// Adds a child node to the successor.  If a node similar to the child has already been added, we use the previously
        /// added node, otherwise we add this. Also, we record the base unit of the child in the set of right context
        /// </summary>
        /// <param name="child">the child to add</param>
        /// <returns>the node (may be different than child if there was already a node attached holding the hmm held by
        ///         child)</returns>
        public UnitNode AddSuccessor(UnitNode child) 
        {
            if (child == null)
                return null;
            UnitNode matchingChild = (UnitNode) GetSuccessor(child.Key);
            if (matchingChild == null) 
            {
                PutSuccessor(child.Key, child);
            } 
            else 
            {
                child = matchingChild;
            }

            return child;
        }


        /// <summary>
        /// Returns the successors for this node
        /// </summary>
        /// <returns>the set of successor nodes</returns>
        public virtual Node[] GetSuccessors() 
        {
            if (_successors is Dictionary<Object, Node>)
            {
                Freeze();
            }
            return (Node[])_successors;
        }
        

        /// <summary>
        /// Returns the string representation for this object
        /// </summary>
        /// <returns>the string representation of the object</returns>
        public override string ToString() 
        {
            return "Node";
        }
    }
}
