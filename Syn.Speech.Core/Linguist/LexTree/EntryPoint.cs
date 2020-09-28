using System.Collections.Generic;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    /// <summary>
    /// Manages a single entry point.
    /// </summary>
    public class EntryPoint
    {
        /// <summary>
        /// the parent tree of this entry point
        /// </summary>
        readonly HMMTree _parent;

        internal Unit BaseUnit;
        internal Dictionary<Unit, Node> UnitToEntryPointMap;
        List<Pronunciation> _singleUnitWords;
        int nodeCount;
        HashSet<Unit> _rcSet;


        /// <summary>
        /// Creates an entry point for the given unit
        /// </summary>
        /// <param name="baseUnit">the EntryPoint is created for this unit</param>
        /// <param name="parent"></param>
        public EntryPoint(Unit baseUnit, HMMTree parent)
        {
            _parent = parent;
            BaseUnit = baseUnit;
            Node = new Node(LogMath.LogZero);
            UnitToEntryPointMap = new Dictionary<Unit, Node>();
            _singleUnitWords = new List<Pronunciation>();
            Probability = LogMath.LogZero;
        }

        /// <summary>
        /// Given a left context get a node that represents a single set of entry points into this unit
        /// </summary>
        /// <param name="leftContext">leftContext the left context of interest</param>
        /// <returns>the node representing the entry point</returns>
        public Node GetEntryPointsFromLeftContext(Unit leftContext)
        {
            return UnitToEntryPointMap[leftContext];
        }

        /// <summary>
        /// Accumulates the probability for this entry point
        /// </summary>
        public void  AddProbability(float probability)
        {
            if (probability > Probability)
            {
                Probability = probability;
            }
        }

        public float Probability { get; private set; }

        /// <summary>
        /// Once we have built the full entry point we can eliminate some fields 
        /// </summary>
        public void Freeze() 
        {
            foreach (Node node in UnitToEntryPointMap.Values) 
            {
                node.Freeze();
            }
            _singleUnitWords = null;
            _rcSet = null;
        }

        /// <summary>
        /// the base node for this entry point
        /// </summary>
        public Node Node { get; private set; }

        /// <summary>
        /// Adds a one-unit word to this entry point. Such single unit words need to be dealt with specially.
        /// </summary>
        /// <param name="p">the pronunciation of the single unit word</param>
        public void AddSingleUnitWord(Pronunciation p) 
        {
            _singleUnitWords.Add(p);
        }

        /// <summary>
        /// Gets the set of possible right contexts that we can transition to from this entry point
        /// </summary>
        /// <returns>the set of possible transition points.</returns>
        private HashSet<Unit> GetEntryPointRC() 
        {
            if (_rcSet == null) 
            {
                _rcSet = new HashSet<Unit>();
                foreach (Node node in Node.GetSuccessorMap().Values)
                {
                    var unitNode = (UnitNode) node;
                    _rcSet.Add(unitNode.BaseUnit);
                }
            }
            return _rcSet;
        }
       
        /// <summary>
        /// A version of createEntryPointMap that compresses common hmms across all entry points.
        /// </summary>
        public void CreateEntryPointMap() 
        {
            Dictionary<IHMM, Node> map = new Dictionary<IHMM, Node>();
            Dictionary<IHMM, HMMNode> singleUnitMap = new Dictionary<IHMM, HMMNode>();

            foreach (Unit lc in _parent.ExitPoints) 
            {
                Node epNode = new Node(LogMath.LogZero);
                foreach (Unit rc in GetEntryPointRC()) 
                {
                    IHMM hmm = _parent.HMMPool.GetHMM(BaseUnit, lc, rc, HMMPosition.Begin);
                    if (hmm == null)
                        continue;
                    Node addedNode=null;
                    if(map.ContainsKey(hmm))
                        addedNode = map[hmm];
                    if (addedNode == null) 
                    {
                        addedNode = epNode.AddSuccessor(hmm, Probability);
                        map.Add(hmm,addedNode);
                    } 
                    else 
                    {
                        epNode.PutSuccessor(hmm, addedNode);
                    }

                    nodeCount++;
                    ConnectEntryPointNode(addedNode, rc);
                }
                ConnectSingleUnitWords(lc, epNode, singleUnitMap);
                Java.Put(UnitToEntryPointMap, lc, epNode);
            }
        }

        /// <summary>
        /// Connects the single unit words associated with this entry point.   The singleUnitWords list contains all
        /// single unit pronunciations that have as their sole unit, the unit associated with this entry point. Entry
        /// points for these words are added to the epNode for all possible left (exit) and right (entry) contexts.
        /// </summary>
        /// <param name="lc">the left context</param>
        /// <param name="epNode">the entry point node</param>
        /// <param name="map"></param>
        private void ConnectSingleUnitWords(Unit lc, Node epNode, Dictionary<IHMM, HMMNode> map) 
        {
            if (!_singleUnitWords.IsEmpty()) 
            {    
                foreach (Unit rc in _parent.EntryPoints) 
                {
                    IHMM hmm = _parent.HMMPool.GetHMM(BaseUnit, lc, rc, HMMPosition.Single);
                    if (hmm == null)
                        continue;
                    HMMNode tailNode=null;
                    if(hmm!=null && map.ContainsKey(hmm))
                        tailNode = map[hmm];

                    if (tailNode == null) 
                    {
                        tailNode = (HMMNode) epNode.AddSuccessor(hmm, Probability);
                        map.Add(hmm, tailNode);
                    } 
                    else 
                    {
                        epNode.PutSuccessor(hmm, tailNode);
                    }
                    WordNode wordNode;
                    tailNode.AddRC(rc);
                    nodeCount++;

                    foreach (Pronunciation p in _singleUnitWords) 
                    {
                        if (p.Word == _parent.Dictionary.GetSentenceStartWord()) 
                        {
                            _parent.InitialNode = new InitialWordNode(p, tailNode);
                        } 
                        else 
                        {
                            float prob = _parent.GetWordUnigramProbability(p.Word);
                            wordNode = tailNode.AddSuccessor(p, prob, _parent.WordNodeMap);
                            if (p.Word == _parent.Dictionary.GetSentenceEndWord()) 
                            {
                                _parent.sentenceEndWordNode = wordNode;
                            }
                        }
                        nodeCount++;
                    }
                }
            }
        }

        /**
        /// Connect the entry points that match the given rc to the given epNode
         *
        /// @param epNode add matching successors here
        /// @param rc     the next unit
         */
        private void ConnectEntryPointNode(Node epNode, Unit rc) 
        {
            foreach (Node node in Node.GetSuccessors()) 
            {
                UnitNode successor = (UnitNode) node;
                if (successor.BaseUnit == rc) 
                {
                    epNode.AddSuccessor(successor);
                }
            }
        }

        /** Dumps the entry point */
        public void Dump() 
        {
            this.LogInfo("EntryPoint " + BaseUnit + " RC Followers: "
                    + GetEntryPointRC().Count);
            int count = 0;
            var rcs = GetEntryPointRC();
            this.LogInfo("    ");
            foreach (Unit rc in rcs) 
            {
                this.LogInfo(Utilities.Pad(rc.Name, 4));
                if (count++ >= 12) {
                    count = 0;
                    this.LogInfo("    ");
                }
            }
            this.LogInfo("");
        }

    }
}
