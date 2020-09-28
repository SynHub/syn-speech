using System;
using System.Collections.Generic;
using System.Text;
using Syn.Speech.Logging;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//REFACTORED
namespace Syn.Speech.Linguist.Language.Grammar
{
    /// <summary>
    /// Classes that implement this interface create grammars. A grammar is represented internally as a graph of {@link
    /// GrammarNode GrammarNodes} linked together by {@link GrammarArc GrammarArcs}. Calling {@link #getInitialNode()
    /// getInitialNode} will return the first node of the grammar graph. To traverse the grammar graph, one should call
    /// GrammarNode.getSuccessors, which will return an array of GrammarArcs, from which you can reach the neighboring
    /// GrammarNodes.
    ///
    /// Note that all grammar probabilities are maintained in LogMath log domain.
    /// </summary>
    public abstract class Grammar: IConfigurable, IGrammarInterface
    {
        /// <summary>
        /// Property to control the the dumping of the grammar
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropShowGrammar = "showGrammar";

        /// <summary>
        /// The default value for PROP_SHOW_GRAMMAR.
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropOptimizeGrammar = "optimizeGrammar";

        /// <summary>
        /// Property to control whether silence words are inserted into the graph.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropAddSilWords = "addSilenceWords";

        /// <summary>
        /// Property to control whether filler words are inserted into the graph.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropAddFillerWords = "addFillerWords";

        /// <summary>
        /// Property that defines the dictionary to use for this grammar.
        /// </summary>
        [S4Component(Type = typeof(IDictionary))]
        public static string PropDictionary = "dictionary";

        // ----------------------------
        // Configuration data
        // -----------------------------
       
        private Boolean _optimizeGrammar = true;
        private Boolean _addSilenceWords;
        private Boolean _addFillerWords;

        private static readonly Word[][] EmptyAlternative = new Word[0][];
        private readonly Random _randomizer = new Random(56); // use fixed initial to make get deterministic random value for testing
        private int _maxIdentity;
        private Boolean _idCheck;

        public Grammar(Boolean showGrammar,Boolean optimizeGrammar,Boolean addSilenceWords, Boolean addFillerWords, IDictionary dictionary ) 
        {

            _optimizeGrammar = optimizeGrammar;
            _addSilenceWords = addSilenceWords;
            _addFillerWords = addFillerWords;
            this.Dictionary = dictionary;
        }

        public Grammar() {

        }

        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */
        public virtual void NewProperties(PropertySheet ps)
        {

            _optimizeGrammar = ps.GetBoolean(PropOptimizeGrammar);

            _addSilenceWords = ps.GetBoolean(PropAddSilWords);
            _addFillerWords = ps.GetBoolean(PropAddFillerWords);

            Dictionary = (IDictionary) ps.GetComponent(PropDictionary);
        }


        /** Create the grammar
        /// @throws java.io.IOException*/
        public void Allocate()
        {
            Dictionary.Allocate();
            NewGrammar();
            Timer timer = TimerPool.GetTimer(this, "grammarLoad");
            timer.Start();
            InitialNode = CreateGrammar();
            timer.Stop();
        }


        /// <summary>
        /// Deallocate resources allocated to this grammar.
        /// </summary>
        public void Deallocate() 
        {
            InitialNode = null;
            GrammarNodes = null;
            Dictionary.Deallocate();
        }

        /// <summary>
        /// Returns the initial node for the grammar
        /// </summary>
        /// <value>the initial grammar node</value>
        public virtual GrammarNode InitialNode { get; protected set; }

        /// <summary>
        ///         
        /// Perform the standard set of grammar post processing. This can include
        /// inserting silence nodes and optimizing out empty nodes
        /// 
        /// </summary>
        protected void PostProcessGrammar() 
        {
            if (_addFillerWords) 
            {
                AddFillerWords();
            } 
            else if (_addSilenceWords) 
            {
                AddSilenceWords();
            }

            if (_optimizeGrammar) 
            {
                OptimizeGrammar();
            }
            DumpStatistics();
        }


        /** Dumps statistics for this grammar */
        public void DumpStatistics() 
        {

            int successorCount = 0;
            this.LogInfo("Num nodes : " + NumNodes);
            foreach (GrammarNode grammarNode in GrammarNodes)
                successorCount += grammarNode.GetSuccessors().Length;

            this.LogInfo("Num arcs  : " + successorCount);
            this.LogInfo("Avg arcs  : "
                    + ((float) successorCount / NumNodes));

        }


        /**
        /// Dump a set of random sentences that fit this grammar
         *
        /// @param path  the name of the file to dump the sentences to
        /// @param count dumps no more than this. May dump less than this depending upon the number of uniqe sentences in the
        ///              grammar.
         */
        public void DumpRandomSentences(String path, int count) 
        {
            try {
                var set = new HashSet<String>();
               
                for (int i = 0; i < count; i++) {
                    string s = GetRandomSentence();
                    if (!set.Contains(s)) 
                    {
                        set.Add(s);
                        this.LogInfo(s);
                    }
                }
            } 
            catch (Exception ioe) 
            {
                this.LogInfo("Can't write random sentences to " + path + ' ' + ioe);
            }
        }


        /**
        /// Dump a set of random sentences that fit this grammar
         *
        /// @param count dumps no more than this. May dump less than this depending upon the number of uniqe sentences in the
        ///              grammar.
         */
        public void DumpRandomSentences(int count) 
        {
            var set = new HashSet<String>();
            for (int i = 0; i < count; i++) 
            {
                string s = GetRandomSentence();
                if (!set.Contains(s)) 
                {
                    set.Add(s);
                }
            }
            List<String> sampleList = new List<String>(set);
            sampleList.Sort();
            foreach (string sentence in sampleList) 
            {
                this.LogInfo(sentence);
            }
        }


        /**
        /// Returns a random sentence that fits this grammar
         *
        /// @return a random sentence that fits this grammar
         */
        public string GetRandomSentence() 
        {
            StringBuilder sb = new StringBuilder();
            GrammarNode node = InitialNode;
            while (!node.IsFinalNode) {
                if (!node.IsEmpty) {
                    Word word = node.GetWord();
                    if (!word.IsFiller)
                        sb.Append(word.Spelling).Append(" ");
                }
                node = SelectRandomSuccessor(node);
            }
            return sb.ToString().Trim();
        }


        /**
        /// Given a node, select a random successor from the set of possible successor nodes
         *
        /// @param node the node
        /// @return a random successor node.
         */
        private GrammarNode SelectRandomSuccessor(GrammarNode node) {
            GrammarArc[] arcs = node.GetSuccessors();

            // select a transition arc with respect to the arc-probabilities (which are log and we don't have a logMath here
            // which makes the implementation a little bit messy
            if (arcs.Length > 1) 
            {
                double[] linWeights = new double[arcs.Length];
                double linWeightsSum = 0;

                double EPS = 1E-10;

                for (int i = 0; i < linWeights.Length; i++) {
                    linWeights[i] = (arcs[0].Probability + EPS) / (arcs[i].Probability + EPS);
                    linWeightsSum += linWeights[i];
                }

                for (int i = 0; i < linWeights.Length; i++) {
                    linWeights[i] /= linWeightsSum;
                }


                double selIndex = _randomizer.NextDouble();
                int index = 0;
                for (int i = 0; selIndex > EPS; i++) 
                {
                    index = i;
                    selIndex -= linWeights[i];
                }

                return arcs[index].GrammarNode;

            } else {
                return arcs[0].GrammarNode;
            }
        }


        /** Dumps the grammar
        /// @param name*/
        public void DumpGrammar(String name) {
            InitialNode.DumpDot(name);
        }

        /**
        /// returns the number of nodes in this grammar
         *
        /// @return the number of nodes
         */

        public int NumNodes
        {
            get { return GrammarNodes.Count; }
        }


        /// <summary>
        /// returns the set of of nodes in this grammar
        /// </summary>
        /// <value>the set of nodes</value>
        public HashSet<GrammarNode> GrammarNodes { get; private set; }


        /** Prepare to create a new grammar */
        protected void NewGrammar() {
            _maxIdentity = 0;
            GrammarNodes = new HashSet<GrammarNode>();
            InitialNode = null;
        }


        /**
        /// Creates a grammar. Subclasses of grammar should implement this method.
         *
        /// @return the initial node for the grammar
        /// @throws java.io.IOException if the grammar could not be loaded
         */
        protected abstract GrammarNode CreateGrammar();


        /**
        /// Create class from reference text (not implemented).
         *
        /// @param bogusText dummy variable
        /// @throws NoSuchMethodException if called with reference sentence
         */
        protected virtual GrammarNode CreateGrammar(String bogusText)
        {
            throw new Exception("Does not create "
                    + "grammar with reference text");
        }


        /**
        /// Gets the dictionary for this grammar
         *
        /// @return the dictionary
         */

        public IDictionary Dictionary { get; protected set; }


        /**
        /// Returns a new GrammarNode with the given set of alternatives.
         *
        /// @param identity the id for this node
        /// @param alts     the set of alternative word lists for this GrammarNode
         */
        protected GrammarNode CreateGrammarNode(int identity, String[][] alts) 
        {
            GrammarNode node;
            Word[][] alternatives = new Word[alts.Length][];
            for (int i = 0; i < alternatives.Length; i++) 
            {
                alternatives[i] = new Word[alts[i].Length];
                for (int j = 0; j < alts[i].Length; j++) 
                {
                    Word word = Dictionary.GetWord(alts[i][j]);
                    // Pronunciation[] pronunciation =
                    // word.getPronunciations(null);
                    if (word == null) {
                        alternatives = EmptyAlternative;
                        break;
                    } else {
                        alternatives[i][j] = word;
                    }
                }
            }
            node = new GrammarNode(identity, alternatives);
            Add(node);

            return node;
        }


        
        /// <summary>
        /// Returns a new GrammarNode with the given single word. If the word is not in the dictionary, an empty node is
        /// created. The grammar id is automatically assigned
        /// </summary>
        /// <param name="word">the word for this grammar node</param>
        /// <returns></returns>
        protected GrammarNode CreateGrammarNode(String word) 
        {
            GrammarNode node = CreateGrammarNode(_maxIdentity + 1, word);
            return node;
        }

        /**
        /// Creates an empty  grammar node in this grammar. The gramar ID is automatically assigned.
         *
        /// @param isFinal if true, this is a final node
        /// @return the grammar node
         */
        internal GrammarNode CreateGrammarNode(Boolean isFinal) {
            return CreateGrammarNode(_maxIdentity + 1, isFinal);
        }


        /// <summary>
        /// Returns a new GrammarNode with the given single word. If the word is not in the dictionary, an empty node is
        /// created
        /// </summary>
        /// <param name="identity">the id for this node</param>
        /// <param name="word">the word for this grammar node</param>
        /// <returns></returns>
        protected GrammarNode CreateGrammarNode(int identity, string word) 
        {
            GrammarNode node;
            Word[][] alternatives = EmptyAlternative;
            Word wordObject = Dictionary.GetWord(word);
            // Pronunciation[] pronunciation = wordObject.getPronunciations(null);
            if (wordObject != null) 
            {
                alternatives = new Word[1][];
                alternatives[0] = new Word[1];
                alternatives[0][0] = wordObject;
                node = new GrammarNode(identity, alternatives);
                Add(node);
            } 
            else 
            {
                node = CreateGrammarNode(identity, false);
                this.LogInfo("Can't find pronunciation for " + word);
            }
            return node;
        }
        
        /**
        /// Creates a grammar node in this grammar with the given identity
         *
        /// @param identity the identity of the node
        /// @param isFinal  if true, this is a final node
        /// @return the grammar node
         */
        protected GrammarNode CreateGrammarNode(int identity, Boolean isFinal) {
            GrammarNode node;
            node = new GrammarNode(identity, isFinal);
            Add(node);
            return node;
        }


        /**
        /// Adds the given grammar node to the set of nodes for this grammar
         *
        /// @param node the grammar node
        /// @throws Error
         */
        private void Add(GrammarNode node) 
        {
            if (node.ID > _maxIdentity) {
                _maxIdentity = node.ID;
            }

            // check to see if there is already a node with the given ID.
            if (_idCheck) {
                foreach (GrammarNode grammarNode in GrammarNodes) 
                {
                    if (grammarNode.ID == node.ID) 
                    {
                        throw new Exception("DUP ID " + grammarNode + " and " + node);
                    }
                }
            }

            GrammarNodes.Add(node);

        }


        /**
        /// Eliminate unnecessary nodes from the grammar. This method goes through the grammar and looks for branches to
        /// nodes that have no words and have only a single exit and bypasses these nodes.
         */
        private void OptimizeGrammar() {
            HashSet<GrammarNode> nodes = GrammarNodes;
            foreach(GrammarNode node in nodes)
                node.Optimize();
        }


        /// <summary>
        /// Adds an optional silence word after every non-filler word in the grammar 
        /// </summary>
        private void AddSilenceWords() 
        {
            HashSet<GrammarNode> nodes = new HashSet<GrammarNode>(GrammarNodes);
            foreach (GrammarNode g in nodes) 
            {
                if (!g.IsEmpty && !g.GetWord().IsFiller) 
                {
                    GrammarNode silNode = CreateGrammarNode(_maxIdentity + 1,
                            Dictionary.GetSilenceWord().Spelling);

                    GrammarNode branchNode = g.SplitNode(_maxIdentity + 1);
                    Add(branchNode);

                    g.Add(silNode, 0.00f);
                    silNode.Add(branchNode, 0.0f);
                    silNode.Add(silNode, 0.0f);
                }
            }
        }


        /// <summary>
        /// Adds an optional filler word loop after every non-filler word in the grammar 
        /// </summary>
        public void AddFillerWords() 
        {
            var nodes = new HashSet<GrammarNode>(GrammarNodes);

            Word[] fillers = GetInterWordFillers();

            if (fillers.Length == 0) 
            {
                return;
            }

            foreach (GrammarNode wordNode in nodes) 
            {
                if (!wordNode.IsEmpty && !wordNode.GetWord().IsFiller) 
                {
                    GrammarNode wordExitNode = wordNode.SplitNode(_maxIdentity + 1);
                    Add(wordExitNode);
                    GrammarNode fillerStart = CreateGrammarNode(false);
                    GrammarNode fillerEnd = CreateGrammarNode(false);
                    fillerEnd.Add(fillerStart, 0.0f);
                    fillerEnd.Add(wordExitNode, 0.0f);
                    wordNode.Add(fillerStart, 0.0f);

                    foreach (Word filler in fillers) 
                    {
                        GrammarNode fnode = CreateGrammarNode(_maxIdentity + 1, filler.Spelling);
                        fillerStart.Add(fnode, 0.0f);
                        fnode.Add(fillerEnd, 0.0f);
                    }
                }
            }
        }


        /**
        /// Gets the set of fillers after filtering out fillers that don't go between words.
         *
        /// @return the set of inter-word fillers
         */
        private Word[] GetInterWordFillers() 
        {
            List<Word> fillerList = new List<Word>();
            Word[] fillers = Dictionary.GetFillerWords();

            foreach (Word fillerWord in fillers) 
            {
                if (fillerWord != Dictionary.GetSentenceStartWord()
                        && fillerWord != Dictionary.GetSentenceEndWord()) 
                {
                    fillerList.Add(fillerWord);
                }
            }
            return fillerList.ToArray();
        }

    }
}
