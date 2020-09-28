using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Language.Grammar
{
    public class FSTGrammar : Grammar
    {
        /// <summary>
        /// The property for the location of the FST n-gram file.
        /// </summary>
        [S4String(DefaultValue = "default.arpa_gram")]
        public const string PropPath = "path";

        // TODO: If this property turns out to be worthwhile, turn this
        // into a full fledged property
        private bool _addInitialSilenceNode;

        // TODO: If this property turns out to be worthwhile, turn this
        // into a full fledged property

        // ------------------------------
        // Configuration data
        // -------------------------------

        private bool _addOptionalSilence;
        private readonly bool _ignoreUnknownTransitions = true;
        private String _path;
        private readonly LogMath _logMath;

        private readonly HashMap<String, GrammarNode> _nodes = new HashMap<String, GrammarNode>();
        private readonly HashSet<GrammarNode> _expandedNodes = new HashSet<GrammarNode>();

        /// <summary>
        ///  Create class from reference text (not implemented).
        /// <param name="bogusText">dummy variable</param>
        /// </summary>
        protected override GrammarNode CreateGrammar(String bogusText)
        {
            throw new MissingMethodException("Does not create grammar with reference text");
        }

        public FSTGrammar(String path, bool showGrammar, bool optimizeGrammar, bool addSilenceWords, bool addFillerWords, IDictionary dictionary)
            : base(showGrammar, optimizeGrammar, addSilenceWords, addFillerWords, dictionary)
        {
            this._path = path;
            _logMath = LogMath.GetLogMath();
        }

        public FSTGrammar()
        {

        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            _path = ps.GetString(PropPath);
        }

        /// <summary>
        /// Creates the grammar.
        /// </summary>
        /// <returns>The initial node for the grammar.</returns>
        protected override GrammarNode CreateGrammar()
        {

            GrammarNode initialNode = null;
            GrammarNode finalNode = null;

            // first pass create the FST nodes
            int maxNodeId = CreateNodes(_path);

            // create the final node:
            finalNode = CreateGrammarNode(++maxNodeId, IDictionary.SilenceSpelling);
            finalNode.SetFinalNode(true);

            // replace each word node with a pair of nodes, which
            // consists of the word node and a new dummy end node, which is
            // for adding null or backoff transitions
            maxNodeId = ExpandWordNodes(maxNodeId);

            ExtendedStreamTokenizer tok = new ExtendedStreamTokenizer(_path, true);

            // Second pass, add all of the arcs

            while (!tok.IsEOF())
            {
                String token;
                tok.Skipwhite();
                token = tok.GetString();

                // System.out.println(token);

                if (token == null)
                {
                    break;

                }
                else if (token.Equals("I"))
                {
                    Debug.Assert(initialNode == null);
                    int initialID = tok.GetInt("initial ID");
                    String nodeName = "G" + initialID;

                    // TODO: FlatLinguist requires the initial grammar node
                    // to contain a single silence. We'll do that for now,
                    // but once the FlatLinguist is fixed, this should be
                    // returned to its former method of creating an empty
                    // initial grammar node
                    //          initialNode = createGrammarNode(initialID, false);

                    initialNode = CreateGrammarNode(initialID, IDictionary.SilenceSpelling);
                    _nodes.Put(nodeName, initialNode);

                    // optionally add a silence node
                    if (_addInitialSilenceNode)
                    {
                        GrammarNode silenceNode = CreateGrammarNode(++maxNodeId, IDictionary.SilenceSpelling);
                        initialNode.Add(silenceNode, LogMath.LogOne);
                        silenceNode.Add(initialNode, LogMath.LogOne);
                    }

                }
                else if (token.Equals("T"))
                {
                    int thisID = tok.GetInt("this id");
                    int nextID = tok.GetInt("next id");

                    GrammarNode thisNode = Get(thisID);
                    GrammarNode nextNode = Get(nextID);

                    // if the source node is an FSTGrammarNode, we want
                    // to join the endNode to the destination node

                    if (HasEndNode(thisNode))
                    {
                        thisNode = GetEndNode(thisNode);
                    }

                    float lnProb = 0f;        // negative natural log
                    String output = tok.GetString();

                    if (output == null || output.Equals(","))
                    {

                        // these are epsilon (meaning backoff) transitions

                        if (output != null && output.Equals(","))
                        {
                            tok.GetString(); // skip the word
                            lnProb = tok.GetFloat("probability");
                        }

                        // if the destination node has been expanded
                        // we actually want to add the backoff transition
                        // to the endNode

                        if (HasEndNode(nextNode))
                        {
                            nextNode = GetEndNode(nextNode);
                        }

                    }
                    else
                    {
                        String word = tok.GetString();     // skip words
                        lnProb = tok.GetFloat("probability");

                        if (_ignoreUnknownTransitions && word.Equals("<unknown>"))
                        {
                            continue;
                        }
                        /*
                        * System.out.println(nextNode + ": " + output);
                        */
                        Debug.Assert(HasWord(nextNode));
                    }

                    thisNode.Add(nextNode, ConvertProbability(lnProb));

                }
                else if (token.Equals("F"))
                {
                    int thisID = tok.GetInt("this id");
                    float lnProb = tok.GetFloat("probability");

                    GrammarNode thisNode = Get(thisID);
                    GrammarNode nextNode = finalNode;

                    if (HasEndNode(thisNode))
                    {
                        thisNode = GetEndNode(thisNode);
                    }

                    thisNode.Add(nextNode, ConvertProbability(lnProb));
                }
            }
            tok.Close();

            Debug.Assert(initialNode != null);

            return initialNode;
        }

        /**
   * Converts the probability from -ln to logmath
   *
   * @param lnProb the probability to convert. Probabilities in the arpa format in negative natural log format. We
   *               convert them to logmath.
   * @return the converted probability in logMath log base
   */
        private float ConvertProbability(float lnProb)
        {
            return _logMath.LnToLog(-lnProb);
        }

        /**
 * Given an id returns the associated grammar node
 *
 * @param id the id of interest
 * @return the grammar node or null if none could be found with the proper id
 */
        private GrammarNode Get(int id)
        {
            String name = "G" + id;
            GrammarNode grammarNode = _nodes.Get(name);
            if (grammarNode == null)
            {
                grammarNode = CreateGrammarNode(id, false);
                _nodes.Put(name, grammarNode);
            }
            return grammarNode;
        }

        /**
   * Reads the FST file in the given path, and creates the nodes in the FST file.
   *
   * @param path the path of the FST file to read
   * @return the highest ID of all nodes
   * @throws java.io.IOException
   */
        private int CreateNodes(String path)
        {
            ExtendedStreamTokenizer tok = new ExtendedStreamTokenizer(path, true);
            int maxNodeId = 0;
            while (!tok.IsEOF())
            {
                tok.Skipwhite();
                String token = tok.GetString();
                if (token == null)
                {
                    break;
                }
                else if (token.Equals("T"))
                {
                    tok.GetInt("src id"); // toss source node
                    int id = tok.GetInt("dest id"); // dest node numb
                    if (id > maxNodeId)
                    {
                        maxNodeId = id;
                    }
                    String word1 = tok.GetString(); // get word
                    if (word1 == null)
                    {
                        continue;
                    }
                    String word2 = tok.GetString(); // get word
                    tok.GetString(); // toss probability
                    String nodeName = "G" + id;
                    GrammarNode node = _nodes.Get(nodeName);
                    if (node == null)
                    {
                        if (word2.Equals(","))
                        {
                            node = CreateGrammarNode(id, false);
                        }
                        else
                        {
                            node = CreateGrammarNode(id, word2);
                        }
                        _nodes.Put(nodeName, node);
                    }
                    else
                    {
                        if (!word2.Equals(","))
                        {
                            /*
                             * if (!word2.equals(getWord(node))) {
                             * System.out.println(node + ": " + word2 + ' ' + getWord(node)); }
                             */
                            Debug.Assert(word2.Equals(GetWord(node)));
                        }
                    }
                }
            }
            tok.Close();
            return maxNodeId;
        }

        /**
  * Expand each of the word nodes into a pair of nodes, as well as adding an optional silence node between the
  * grammar node and its end node.
  *
  * @param maxNodeID the node ID to start with for the new nodes
  * @return the last (or maximum) node ID
  */
        private int ExpandWordNodes(int maxNodeID)
        {
            var allNodes = _nodes.Values;
            String[][] silence = { new[] { IDictionary.SilenceSpelling } };
            foreach (GrammarNode node in allNodes)
            {
                // if it has at least one word, then expand the node
                if (node.GetNumAlternatives() > 0)
                {
                    GrammarNode endNode = CreateGrammarNode(++maxNodeID, false);
                    node.Add(endNode, LogMath.LogOne);
                    // add an optional silence
                    if (_addOptionalSilence)
                    {
                        GrammarNode silenceNode = CreateGrammarNode(++maxNodeID,
                                silence);
                        node.Add(silenceNode, LogMath.LogOne);
                        silenceNode.Add(endNode, LogMath.LogOne);
                    }
                    _expandedNodes.Add(node);
                }
            }
            return maxNodeID;
        }

        /**
 * Determines if the node has a word
 *
 * @param node the grammar node of interest
 * @return true if the node has a word
 */
        private bool HasWord(GrammarNode node)
        {
            return (node.GetNumAlternatives() > 0);
        }

        /**
            * Gets the word from the given grammar ndoe
            *
            * @param node the node of interest
            * @return the word (or null if the node has no word)
            */
        private String GetWord(GrammarNode node)
        {
            String word = null;
            if (node.GetNumAlternatives() > 0)
            {
                Word[][] alternatives = node.Alternatives;
                word = alternatives[0][0].Spelling;
            }
            return word;
        }

        /**
         * Determines if the given node has an end node associated with it.
         *
         * @param node the node of interest
         * @return <code>true</code> if the given node has an end node.
         */
        private bool HasEndNode(GrammarNode node)
        {
            return (_expandedNodes.Contains(node));
        }

        /**
         * Retrieves the end node associated with the given node
         *
         * @param node the node of interest
         * @return the ending node or null if no end node is available
         */
        private GrammarNode GetEndNode(GrammarNode node)
        {
            GrammarArc[] arcs = node.GetSuccessors();
            Debug.Assert(arcs != null && arcs.Length > 0);
            return arcs[0].GrammarNode;
        }
    }
}
