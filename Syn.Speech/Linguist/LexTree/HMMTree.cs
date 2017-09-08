using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Syn.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Linguist.Language.NGram;
using Syn.Speech.Util;

namespace Syn.Speech.Linguist.LexTree
{
    /// <summary>
    /// Represents the vocabulary as a lex tree with nodes in the tree representing either words (WordNode) or units
    /// (HMMNode). HMMNodes may be shared.
    /// </summary>
    public class HMMTree
    {
        public  HMMPool HMMPool;
        public  IDictionary Dictionary;
        private LanguageModel _lm;
        private readonly Boolean _addFillerWords;
        private const Boolean AddSilenceWord = true;
        public HashSet<Unit> EntryPoints = new HashSet<Unit>();
        public HashSet<Unit> ExitPoints = new HashSet<Unit>();
        private HashSet<Word> _allWords;
        private EntryPointTable _entryPointTable;
        private Boolean _debug;
        private readonly float _languageWeight;
        private readonly HashMap<Object, HMMNode[]> _endNodeMap;
        public readonly HashMap<Pronunciation, WordNode> WordNodeMap;
        public  WordNode sentenceEndWordNode;


        /**
        /// Creates the HMMTree
         *
        /// @param pool           the pool of HMMs and units
        /// @param dictionary     the dictionary containing the pronunciations
        /// @param lm             the source of the set of words to add to the lex tree
        /// @param addFillerWords if <code>false</code> add filler words
        /// @param languageWeight the languageWeight
         */
        public HMMTree(HMMPool pool, IDictionary dictionary, LanguageModel lm,Boolean addFillerWords, float languageWeight) 
        {
            HMMPool = pool;
            Dictionary = dictionary;
            _lm = lm;
            _endNodeMap = new HashMap<Object, HMMNode[]>();
            WordNodeMap = new HashMap<Pronunciation, WordNode>();
            _addFillerWords = addFillerWords;
            _languageWeight = languageWeight;
        
            TimerPool.GetTimer(this,"Create HMM Tree").Start();
            Compile();
            TimerPool.GetTimer(this,"Create HMM Tree").Stop();
        }


        /**
        /// Given a base unit and a left context, return the set of entry points into the lex tree
         *
        /// @param lc   the left context
        /// @param base the center unit
        /// @return the set of entry points
         */
        public Node[] GetEntryPoint(Unit lc, Unit _base) 
        {
            EntryPoint ep = _entryPointTable.GetEntryPoint(_base);
            return ep.GetEntryPointsFromLeftContext(lc).GetSuccessors();
        }


        /**
        /// Gets the  set of hmm nodes associated with the given end node
         *
        /// @param endNode the end node
        /// @return an array of associated hmm nodes
         */
        public HMMNode[] GetHMMNodes(EndNode endNode) 
        {
            HMMNode[] results = _endNodeMap.Get(endNode.Key);
            if (results == null) 
            {
                // System.out.println("Filling cache for " + endNode.getKey()
                //        + " size " + endNodeMap.size());
                HashMap<IHMM, HMMNode> resultMap = new HashMap<IHMM, HMMNode>();
                Unit baseUnit = endNode.BaseUnit;
                Unit lc = endNode.LeftContext;
                foreach (Unit rc in EntryPoints) 
                {
                    IHMM hmm = HMMPool.GetHMM(baseUnit, lc, rc, HMMPosition.End);
                    HMMNode hmmNode = resultMap.Get(hmm);
                    if (hmmNode == null) 
                    {
                        hmmNode = new HMMNode(hmm, LogMath.LogOne);
                        resultMap.Add(hmm, hmmNode);
                    }
                    hmmNode.AddRC(rc);
                    foreach (Node node in endNode.GetSuccessors()) 
                    {
                        WordNode wordNode = (WordNode)node;
                        hmmNode.AddSuccessor(wordNode);
                    }
                }

                // cache it
                results = resultMap.Values.ToArray();
                _endNodeMap.Add(endNode.Key, results);
            }

            // System.out.println("GHN: " + endNode + " " + results.length);
            return results;
        }


        /**
        /// Returns the word node associated with the sentence end word
         *
        /// @return the sentence end word node
         */

        public WordNode SentenceEndWordNode
        {
            get
            {
                Debug.Assert(sentenceEndWordNode != null);
                return sentenceEndWordNode;
            }
        }


        //    private Object getKey(EndNode endNode) {
    //        Unit base = endNode.getBaseUnit();
    //        Unit lc = endNode.getLeftContext();
    //        return null;
    //    }


        /** Compiles the vocabulary into an HMM Tree */
        private void Compile() 
        {
            CollectEntryAndExitUnits();
            _entryPointTable = new EntryPointTable(EntryPoints,this);
            AddWords();
            _entryPointTable.CreateEntryPointMaps();
            Freeze();
        }


        /** Dumps the tree */
        void DumpTree() 
        {
            this.LogInfo("Dumping Tree ...");
            Dictionary<Node, Node> dupNode = new Dictionary<Node, Node>();
            DumpTree(0, InitialNode, dupNode);
            this.LogInfo("... done Dumping Tree");
        }


        /**
        /// Dumps the tree
         *
        /// @param level   the level of the dump
        /// @param node    the root of the tree to dump
        /// @param dupNode map of visited nodes
         */
        private void DumpTree(int level, Node node, Dictionary<Node, Node> dupNode) 
        {
            if (dupNode[node] == null) 
            {
                dupNode.Add(node, node);
                this.LogInfo(Utilities.Pad(level) + node);
                if (!(node is WordNode)) 
                {
                    foreach (Node nextNode in node.GetSuccessors()) 
                    {
                        DumpTree(level + 1, nextNode, dupNode);
                    }
                }
            }
        }


        /// <summary>
        /// Collects all of the entry and exit points for the vocabulary.
        /// </summary>
        private void CollectEntryAndExitUnits()
        {
            var words = GetAllWords();
            foreach (Word word in words)
            {
                for (int j = 0; j < word.GetPronunciations().Length; j++)
                {
                    Pronunciation p = word.GetPronunciations()[j];
                    Unit first = p.Units[0];
                    Unit last = p.Units[p.Units.Length - 1];
                    EntryPoints.Add(first);
                    ExitPoints.Add(last);
                }
            }

            //foreach (Word word in getAllWords()) 
            //{
            //    foreach (Pronunciation p in word.getPronunciations())
            //    {
            //        entryPoints.Add(p.getUnits().First());
            //        exitPoints.Add(p.getUnits().Last());
            //    }
            //}

#if DEBUG
                this.LogInfo("Entry Points: " + EntryPoints.Count);
                this.LogInfo("Exit Points: " + ExitPoints.Count);
#endif
        }


        /**
        /// Called after the lex tree is built. Frees all temporary structures. After this is called, no more words can be
        /// added to the lex tree.
         */
        private void Freeze() 
        {
            _entryPointTable.Freeze();
            Dictionary = null;
            _lm = null;
            ExitPoints = null;
            _allWords = null;
            WordNodeMap.Clear();
            _endNodeMap.Clear();
        }


        /// <summary>
        /// Adds the given collection of words to the lex tree
        /// </summary>
        private void AddWords()
        {
            var words = GetAllWords();
            foreach (Word word in words) 
            {
                AddWord(word);
            }
        }


        /**
        /// Adds a single word to the lex tree
         *
        /// @param word the word to add
         */
        private void AddWord(Word word) 
        {

            float prob = GetWordUnigramProbability(word);
          
            foreach (Pronunciation pronunciation in word.GetPronunciations()) 
            {
                AddPronunciation(pronunciation, prob);
            }
        }


        /**
        /// Adds the given pronunciation to the lex tree
         *
        /// @param pronunciation the pronunciation
        /// @param probability   the unigram probability
         */
        private void AddPronunciation(Pronunciation pronunciation,float probability) 
        {
            Unit baseUnit;
            Unit lc;
            Unit rc;
            Node curNode;
            WordNode wordNode;

            Unit[] units = pronunciation.Units;
            baseUnit = units[0];
            EntryPoint ep = _entryPointTable.GetEntryPoint(baseUnit);

            ep.AddProbability(probability); 

            if (units.Length > 1) 
            {
                curNode = ep.Node;
                lc = baseUnit;
                for (int i = 1; i < units.Length - 1; i++) 
                {
                    baseUnit = units[i];
                    rc = units[i + 1];
                    IHMM hmm = HMMPool.GetHMM(baseUnit, lc, rc, HMMPosition.Internal);
                    if (hmm == null) 
                    {
                        if(_debug)
                            Trace.TraceError("Missing HMM for unit " + baseUnit.Name + " with lc=" + lc.Name + " rc=" + rc.Name);
                    } 
                    else {
                        curNode = curNode.AddSuccessor(hmm, probability);
                    }
                    lc = baseUnit;          // next lc is this baseUnit
                }

                // now add the last unit as an end unit
                baseUnit = units[units.Length - 1];
                EndNode endNode = new EndNode(baseUnit, lc, probability);
                curNode = curNode.AddSuccessor(endNode, probability);
                wordNode = curNode.AddSuccessor(pronunciation, probability , WordNodeMap);
                if (wordNode.GetWord().IsSentenceEndWord) {
                    sentenceEndWordNode = wordNode;
                }
            } else {
                ep.AddSingleUnitWord(pronunciation);
            }
        }

    
        /**
        /// Gets the unigram probability for the given word
         *
        /// @param word the word
        /// @return the unigram probability for the word.
         */
        public float GetWordUnigramProbability(Word word) 
        {
            float prob = LogMath.LogOne;
            if (!word.IsFiller) 
            {
                Word[] wordArray = new Word[1];
                wordArray[0] = word;
                prob = _lm.GetProbability((new WordSequence(wordArray)));
                // System.out.println("gwup: " + word + " " + prob);
                prob *= _languageWeight;
            }
            return prob;
        }


        /// <summary>
        /// Returns the entire set of words, including filler words
        /// </summary>
        /// <returns>the set of all words (as Word objects)</returns>
        private HashSet<Word> GetAllWords() 
        {
            if (_allWords == null) 
            {
                _allWords = new HashSet<Word>();
                foreach (String spelling in _lm.Vocabulary) 
                {
                    Word word = Dictionary.GetWord(spelling);
                    //TODO: For some reason CMUSPHINX adds a filler here
                    if (word != null ) 
                    {
                        _allWords.Add(word);
                    }
                }

                if (_addFillerWords) 
                {
                    Java.AddAll(_allWords, Dictionary.GetFillerWords());
                } 
                else if (AddSilenceWord) 
                {
                    _allWords.Add(Dictionary.GetSilenceWord());
                }
            }

            return _allWords;
        }

        /**
        /// Returns the initial node for this lex tree
         *
        /// @return the initial lex node
         */

        public InitialWordNode InitialNode { get; set; }
    }
}
