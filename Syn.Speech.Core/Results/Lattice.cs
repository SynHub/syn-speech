using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Syn.Speech.Decoders.Search;
using Syn.Speech.Helper;
using Syn.Speech.Linguist;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Results
{
    /**
    /// <p/>
    /// Provides recognition lattice results. Lattices are created from {@link edu.cmu.sphinx.result.Result Results} which
    /// can be partial or final. </p>
    /// <p/>
    /// Lattices describe all theories considered by the Recognizer that have not been pruned out.  Lattices are a directed
    /// graph containing {@link edu.cmu.sphinx.result.Node Nodes} and {@link edu.cmu.sphinx.result.Edge Edges}. A Node that
    /// corresponds to a theory that a word was spoken over a particular period of time.  An Edge that corresponds to the
    /// score of one word following another.  The usual result transcript is the sequence of Nodes though the Lattice with
    /// the best scoring path. Lattices are a useful tool for analyzing "alternate results". </p>
    /// <p/>
    /// A Lattice can be created from a Result that has a full token tree (with its corresponding
    /// AlternativeHypothesisManager). Currently, only the {@link edu.cmu.sphinx.decoder.search.WordPruningBreadthFirstSearchManager}
    /// has an AlternativeHypothesisManager. Furthermore, the lattice construction code currently only works for linguists
    /// where the {@link edu.cmu.sphinx.linguist.WordSearchState} returns false on the <code>isWordStart</code> method, i.e.,
    /// where the word states appear at the end of the word in the linguist. <i>Therefore, lattices should only be created
    /// from Result from the {@link edu.cmu.sphinx.linguist.lextree.LexTreeLinguist} and the {@link
    /// edu.cmu.sphinx.decoder.search.WordPruningBreadthFirstSearchManager}. </i> </p>
    /// <p/>
    /// Lattices can also be created from a collapsed {@link edu.cmu.sphinx.decoder.search.Token} tree and its
    /// AlternativeHypothesisManager. This is what 'collapsed' means. Normally, between two word tokens is a series of tokens
    /// for other types of states, such as unit or HMM states. Using 'W' for word tokens, 'U' for unit tokens, 'H' for HMM
    /// tokens, a token chain can look like: </p>
    /// <pre>
    /// W - U - H - H - H - H - U - H - H - H - H - W
    /// </pre>
    /// <p/>
    /// Usually, HMM tokens contains acoustic scores, and word tokens contains language scores. If we want to know the total
    /// acoustic and language scores between any two words, it is unnecessary to keep around the unit and HMM tokens.
    /// Therefore, all their acoustic and language scores are 'collapsed' into one token, so that it will look like: </p>
    /// <pre>
    /// W - P - W
    /// </pre>
    /// <p/>
    /// where 'P' is a token that represents the path between the two words, and P contains the acoustic and language scores
    /// between the two words. It is this type of collapsed token tree that the Lattice class is expecting. Normally, the
    /// task of collapsing the token tree is done by the {@link edu.cmu.sphinx.decoder.search.WordPruningBreadthFirstSearchManager}.
    /// A collapsed token tree can look like: </p>
    /// <pre>
    ///                             "cat" - P - &lt;/s&gt;
    ///                            /
    ///                           P
    ///                          /
    /// &lt;s&gt; - P - "a" - P - "big"
    ///                          \
    ///                           P
    ///                            \
    ///                             "dog" - P - &lt;/s&gt;
    /// </pre>
    /// <p/>
    /// When a Lattice is constructed from a Result, the above collapsed token tree together with the alternate hypothesis of
    /// "all" instead of "a", will be converted into a Lattice that looks like the following:
    /// <pre>
    ///       "a"           "cat"
    ///     /     \        /     \
    /// &lt;s&gt;          "big"         - &lt;/s&gt;
    ///     \     /        \     /
    ///      "all"          "dog"
    /// </pre>
    /// <p/>
    /// Initially, a lattice can have redundant nodes, i.e., nodes referring to the same word and that originate from the
    /// same parent node. These nodes can be collapsed using the {@link LatticeOptimizer}. </p>
     */
    public class Lattice
    {
        protected HashSet<Edge> Edges;
        protected internal HashMap<String, Node> Nodes;
        protected double LogBase;
        protected LogMath LogMath;
        private readonly HashSet<Token> _visitedWordTokens;
        private readonly AlternateHypothesisManager _loserManager;


        /** Create an empty Lattice. */
        public Lattice()
        {
            Edges = new HashSet<Edge>();
            Nodes = new HashMap<string, Node>();
            LogMath = LogMath.GetLogMath();

        }

        /**
        /// Create a Lattice from a Result.
        /// <p/>
        /// The Lattice is created from the Token tree referenced by the Result. The Lattice is then optimized to all
        /// collapse equivalent paths.
         *
        /// @param result the result to convert into a lattice
         */
        public Lattice(Result result)
            : this()
        {
            _visitedWordTokens = new HashSet<Token>();
            _loserManager = result.AlternateHypothesisManager;
            if (_loserManager != null)
            {
                _loserManager.Purge();
            }

            var tokens = result.GetBestFinalToken() == null ? (IEnumerable<Token>)result.ActiveTokens : result.ResultTokens;

            foreach (var token in tokens)
            {
                var newToken = token.Predecessor;
                while (newToken != null && !newToken.IsWord)
                {
                    newToken = newToken.Predecessor;
                }
                //Debug.Assert(newToken != null && newToken.getWord().isSentenceEndWord());
                if (TerminalNode == null)
                {
                    TerminalNode = new Node(GetNodeId(result.GetBestToken()),
                            token.GetWord(), -1, -1);
                    InitialNode = TerminalNode;
                    AddNode(TerminalNode);
                }
                CollapseWordToken(newToken);
            }
        }


        /**
        /// Returns the node corresponding to the given word token.
         *
        /// @param token the token which we want a node of
        /// @return the node of the given token
         */
        private Node GetNode(Token token)
        {
            if (token.GetWord().IsSentenceEndWord)
            {
                return TerminalNode;
            }
            var node = Nodes.Get(GetNodeId(token));
            if (node == null)
            {
                var wordState =
                        (IWordSearchState)token.SearchState;

                var startFrame = -1;
                var endFrame = -1;

                if (wordState.IsWordStart())
                {
                    startFrame = token.FrameNumber;
                }
                else
                {
                    endFrame = token.FrameNumber;
                }

                node = new Node(GetNodeId(token), token.GetWord(),
                        startFrame, endFrame);
                AddNode(node);
            }
            return node;
        }


        /**
        /// Collapse the given word-ending token. This means collapsing all the unit
        /// and HMM tokens that correspond to the word represented by this token into
        /// an edge of the lattice.
        /// 
        /// @param token
        ///            the word-ending token to collapse
         */
        private void CollapseWordToken(Token token)
        {
            Debug.Assert(token != null);
            if (_visitedWordTokens.Contains(token))
            {
                return;
            }
            _visitedWordTokens.Add(token);

            CollapseWordPath(GetNode(token), token.Predecessor,
                    token.AcousticScore + token.InsertionScore,
                    token.LanguageScore);

            if (_loserManager != null
                    && _loserManager.HasAlternatePredecessors(token))
            {
                foreach (var loser in _loserManager.GetAlternatePredecessors(token))
                {
                    CollapseWordPath(GetNode(token), loser,
                            token.AcousticScore, token.LanguageScore);
                }
            }
        }

        /**
        /// @param parentWordNode
        ///            the 'toNode' of the returned edge
        /// @param token
        ///            the predecessor token of the token represented by the
        ///            parentWordNode
        /// @param acousticScore
        ///            the acoustic score until and including the parent of token
        /// @param languageScore
        ///            the language score until and including the parent of token
         */
        private void CollapseWordPath(Node parentWordNode, Token token, float acousticScore, float languageScore)
        {

            if (token == null)
                return;

            if (token.IsWord)
            {
                /*
                /// If this is a word, create a Node for it, and then create an edge
                /// from the Node to the parentWordNode
                 */
                var fromNode = GetNode(token);
                AddEdge(fromNode, parentWordNode, acousticScore,
                        languageScore);
                if (token.Predecessor != null)
                {
                    /* Collapse the token sequence ending in this token. */
                    CollapseWordToken(token);
                }
                else
                {
                    /* we've reached the sentence start token */
                    //Debug.Assert(token.getWord().isSentenceStartWord());
                    InitialNode = fromNode;
                }
                return;
            }

            /*
            /// If a non-word token, just add the acoustic and language scores to the
            /// current totals, and then move on to the predecessor token. Fast
            /// forward through the not so interesting states to save stack space.
             */
            while (true)
            {
                acousticScore += token.AcousticScore
                        + token.InsertionScore;
                languageScore += token.LanguageScore;
                var preToken = token.Predecessor;

                if (preToken == null)
                    return;

                if (preToken.IsWord
                        || (_loserManager != null && _loserManager
                                .HasAlternatePredecessors(token)))
                    break;
                token = preToken;
            }

            CollapseWordPath(parentWordNode, token.Predecessor, acousticScore,
                    languageScore);

            /* Traverse the path(s) for the loser token(s). */
            if (_loserManager != null
                    && _loserManager.HasAlternatePredecessors(token))
            {
                foreach (var loser in _loserManager.GetAlternatePredecessors(token))
                {
                    CollapseWordPath(parentWordNode, loser, acousticScore,
                            languageScore);
                }
            }
        }

        /**
        /// Returns an ID for the Node associated with the given token.
         *
        /// @param token the token associated with the Node
        /// @return an ID for the Node
         */
        private static string GetNodeId(Token token)
        {
            return token.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }


        /**
        /// Create a Lattice from a LAT file.  LAT files are created by the method Lattice.dump()
         *
        /// @param fileName
         */
        public Lattice(String fileName)
        {
            try
            {
                Console.WriteLine(@"Loading from " + fileName);

                // load the nodes
                var numberReader = new LineNumberReader(new StreamReader(fileName));
                string line;
                while ((line = numberReader.ReadLine()) != null)
                {
                    var tokens = new StringTokenizer(line);
                    if (tokens.hasMoreTokens())
                    {
                        var type = tokens.nextToken();

                        if (type.Equals("edge:"))
                        {
                            Edge.Load(this, tokens);
                        }
                        else if (type.Equals("node:"))
                        {
                            Node.Load(this, tokens);
                        }
                        else if (type.Equals("initialNode:"))
                        {
                            InitialNode = GetNode(tokens.nextToken());
                        }
                        else if (type.Equals("terminalNode:"))
                        {
                            TerminalNode = GetNode(tokens.nextToken());
                        }
                        else if (type.Equals("logBase:"))
                        {
                            LogBase = Double.Parse(tokens.nextToken(), CultureInfo.InvariantCulture.NumberFormat);
                        }
                        else
                        {
                            numberReader.Close();
                            throw new Exception("SYNTAX ERROR: " + fileName +
                                '[' + numberReader.LineNumber + "] " + line);
                        }
                    }
                }
                numberReader.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        public static Lattice ReadSlf(String fileName)
        {
            var lattice = new Lattice();
            var @in = new LineNumberReader(new StreamReader(fileName));
            String line;
            var readingNodes = false;
            var readingEdges = false;
            var startIdx = 0;
            var endIdx = 1;
            var lmscale = 9.5;
            while ((line = @in.ReadLine()) != null)
            {
                if (line.Contains("Node definitions"))
                {
                    readingEdges = false;
                    readingNodes = true;
                    continue;
                }
                if (line.Contains("Link definitions"))
                {
                    readingEdges = true;
                    readingNodes = false;
                    continue;
                }
                if (line.StartsWith("#"))
                    //skip commented line
                    continue;
                if (readingNodes)
                {
                    //reading node info, format:
                    //I=id   t=start_time_sec   W=word_transcription
                    var parts = line.Split("\\s+");
                    if (parts.Length != 3 || !parts[0].StartsWith("I=") || !parts[1].StartsWith("t=") || !parts[2].StartsWith("W="))
                    {
                        @in.Close();
                        throw new IOException("Unknown node definition: " + line);
                    }
                    var idx = Convert.ToInt32(parts[0].Substring(2), CultureInfo.InvariantCulture.NumberFormat);
                    //convert to milliseconds inplace
                    var beginTime = (long)(Convert.ToDouble(parts[1].Substring(2), CultureInfo.InvariantCulture.NumberFormat) * 1000);
                    var wordStr = parts[2].Substring(2);
                    var isFiller = false;
                    if (idx == startIdx || wordStr.Equals("!ENTER"))
                    {
                        wordStr = "<s>";
                        isFiller = true;
                    }
                    if (idx == endIdx || wordStr.Equals("!EXIT"))
                    {
                        wordStr = "</s>";
                        isFiller = true;
                    }
                    if (wordStr.Equals("!NULL"))
                    {
                        wordStr = "<sil>";
                        isFiller = true;
                    }
                    if (wordStr.StartsWith("["))
                        isFiller = true;
                    var word = new Word(wordStr, new Pronunciation[0], isFiller);
                    var node = lattice.AddNode(idx.ToString(CultureInfo.InvariantCulture), word, beginTime, -1);
                    if (wordStr.Equals("<s>"))
                        lattice.InitialNode = node;
                    if (wordStr.Equals("</s>"))
                        lattice.TerminalNode = node;
                }
                else if (readingEdges)
                {
                    //reading edge info, format:
                    //J=id   S=from_node   E=to_node   a=acoustic_score   l=language_score
                    var parts = line.Split("\\s+");
                    if (parts.Length != 5 || !parts[1].StartsWith("S=") || !parts[2].StartsWith("E=")
                            || !parts[3].StartsWith("a=") || !parts[4].StartsWith("l="))
                    {
                        @in.Close();
                        throw new IOException("Unknown edge definition: " + line);
                    }
                    var fromId = parts[1].Substring(2);
                    var toId = parts[2].Substring(2);
                    var ascore = Convert.ToDouble(parts[3].Substring(2), CultureInfo.InvariantCulture.NumberFormat);
                    var lscore = Convert.ToDouble(parts[4].Substring(2), CultureInfo.InvariantCulture.NumberFormat) * lmscale;
                    lattice.AddEdge(lattice.Nodes.Get(fromId), lattice.Nodes.Get(toId), ascore, lscore);
                }
                else
                {
                    //reading header here if needed
                    if (line.StartsWith("start="))
                        startIdx = Convert.ToInt32(line.Replace("start=", ""), CultureInfo.InvariantCulture.NumberFormat);
                    if (line.StartsWith("end="))
                        endIdx = Convert.ToInt32(line.Replace("end=", ""), CultureInfo.InvariantCulture.NumberFormat);
                    if (line.StartsWith("lmscale="))
                        lmscale = Convert.ToDouble(line.Replace("lmscale=", ""), CultureInfo.InvariantCulture.NumberFormat);
                }
            }
            foreach (var node in lattice.Nodes.Values)
                //calculate end time of nodes depending successors begin time
                foreach (var edge in node.LeavingEdges)
                    if (node.EndTime < 0 || node.EndTime > edge.ToNode.BeginTime)
                        node.EndTime = edge.ToNode.BeginTime;
            @in.Close();
            return lattice;
        }


        /**
        /// Add an edge from fromNode to toNode.  This method creates the Edge object and does all the connecting
         *
        /// @param fromNode
        /// @param toNode
        /// @param acousticScore
        /// @param lmScore
        /// @return the new Edge
         */
        public Edge AddEdge(Node fromNode, Node toNode, double acousticScore, double lmScore)
        {
            var e = new Edge(fromNode, toNode, acousticScore, lmScore);
            fromNode.AddLeavingEdge(e);
            toNode.AddEnteringEdge(e);
            Edges.Add(e);
            return e;
        }


        /**
        /// Add a Node that represents the theory that a given word was spoken over a given period of time.
         *
        /// @param word
        /// @param beginTime
        /// @param endTime
        /// @return the new Node
         */
        public Node AddNode(Word word, int beginTime, int endTime)
        {
            var n = new Node(word, beginTime, endTime);
            AddNode(n);
            return n;
        }

        /**
        /// Add a Node with a given ID that represents the theory that a given word was spoken over a given period of time.
        /// This method is used when loading Lattices from .LAT files.
         *
        /// @param word
        /// @param beginTime
        /// @param endTime
        /// @return the new Node
         */
        protected Node AddNode(String id, Word word, long beginTime, long endTime)
        {
            var n = new Node(id, word, beginTime, endTime);
            AddNode(n);
            return n;
        }


        /**
        /// Add a Node with a given ID that represents the theory that a given word was spoken over a given period of time.
        /// This method is used when loading Lattices from .LAT files.
         *
        /// @param word
        /// @param beginTime
        /// @param endTime
        /// @return the new Node
         */
        public Node AddNode(String id, string word, long beginTime, long endTime)
        {
            var w = new Word(word, new Pronunciation[0], false);
            return AddNode(id, w, beginTime, endTime);
        }


        /**
        /// Add a Node corresponding to a Token from the result Token tree. Usually, the Token should reference a search
        /// state that is a WordSearchState, although other Tokens may be used for debugging.
         *
        /// @param token
        /// @return the new Node
         */
        protected Node AddNode(Token token, int beginTime, int endTime)
        {
            Debug.Assert(token.SearchState is IWordSearchState);
            var word = ((IWordSearchState)(token.SearchState)).Pronunciation.Word;
            return AddNode(token.GetHashCode().ToString(CultureInfo.InvariantCulture), word, beginTime, endTime);
        }


        /**
        /// Test to see if the Lattice contains an Edge
         *
        /// @param edge
        /// @return true if yes
         */
        public Boolean HasEdge(Edge edge)
        {
            return Edges.Contains(edge);
        }


        /**
        /// Test to see if the Lattice contains a Node
         *
        /// @param node
        /// @return true if yes
         */
        public Boolean HasNode(Node node)
        {
            return HasNode(node.Id);
        }


        /**
        /// Test to see if the Lattice already contains a Node corresponding to a given Token.
         *
        /// @param ID the ID of the Node to find
        /// @return true if yes
         */
        public Boolean HasNode(String id)
        {
            return Nodes.ContainsKey(id);
        }


        /**
        /// Add a Node to the set of all Nodes
         *
        /// @param n
         */
        protected void AddNode(Node n)
        {
            Debug.Assert(!HasNode(n.Id));
            Nodes.Add(n.Id, n);
        }


        /**
        /// Remove a Node from the set of all Nodes
         *
        /// @param n
         */
        protected void RemoveNode(Node n)
        {
            Debug.Assert(HasNode(n.Id));
            Nodes.Remove(n.Id);
        }


        /**
        /// Get the Node associated with an ID
         *
        /// @param id
        /// @return the Node
         */
        public Node GetNode(String id)
        {
            return (Nodes.Get(id));
        }


        /**
        /// Get a copy of the Collection of all Nodes. Used by LatticeOptimizer to avoid Concurrent modification of the nodes
        /// list.
         *
        /// @return a copy of the collection of Nodes
         */
        public ICollection<Node> GetCopyOfNodes()
        {
            return new List<Node>(Nodes.Values);
        }


        /**
        /// Get the Collection of all Nodes.
         *
        /// @return the collection of all Nodes
         */
        public ICollection<Node> GetNodes()
        {
            return Nodes.Values;
        }


        /**
        /// Remove an Edge from the set of all Edges.
         *
        /// @param e
         */
        protected void RemoveEdge(Edge e)
        {
            Edges.Remove(e);
        }


        /**
        /// Get the set of all Edges.
         *
        /// @return the set of all edges
         */
        public HashSet<Edge> GetEdges()
        {
            return Edges;
        }


        /**
        /// Dump the Lattice in the form understood by AiSee (a graph visualization tool).  See http://www.AbsInt.com
         *
        /// @param fileName
        /// @param title
         */
        public void DumpAISee(String fileName, string title)
        {
            Debug.Print("Dumping " + title + " to " + fileName);
            var f = new StreamWriter(fileName);
            f.Write("graph: {\n");
            f.Write("title: \"" + title + "\"\n");
            f.Write("display_edge_labels: yes\n");
            /*
                f.write( "colorentry 32: 25 225 0\n");
                f.write( "colorentry 33: 50 200 0\n");
                f.write( "colorentry 34: 75 175 0\n");
                f.write( "colorentry 35: 100 150 0\n");
                f.write( "colorentry 36: 125 125 0\n");
                f.write( "colorentry 37: 150 100 0\n");
                f.write( "colorentry 38: 175 75 0\n");
                f.write( "colorentry 39: 200 50 0\n");
                f.write( "colorentry 40: 225 25 0\n");
                f.write( "colorentry 41: 250 0 0\n");
                f.write( "color: black\n");
                f.write( "orientation: left_to_right\n");
                f.write( "xspace: 10\n");
                f.write( "yspace: 10\n");
                */

            foreach (var node in Nodes.Values)
            {
                node.DumpAISee(f);
            }
            foreach (var edge in Edges)
            {
                edge.DumpAISee(f);
            }
            f.Write("}\n");
            f.Close();
        }

        /**
        /// Dump the Lattice in the form understood by Graphviz. See http://graphviz.org
         *
        /// @param fileName
        /// @param title
         */
        public void DumpDot(String fileName, string title)
        {
            try
            {
                Console.WriteLine("Dumping " + title + " to " + fileName);
                var f = new StreamWriter(fileName);
                f.Write("digraph \"" + title + "\" {\n");
                f.Write("rankdir = LR\n");

                foreach (var node in Nodes.Values)
                {
                    node.DumpDot(f);
                }
                foreach (var edge in Edges)
                {
                    edge.DumpDot(f);
                }
                f.Write("}\n");
                f.Close();
            }
            catch (IOException e)
            {
                throw e;
            }
        }


        public void DumpSlf(StreamWriter w)
        {
            w.Write("VERSION=1.1\n");
            w.Write("UTTERANCE=test\n");
            w.Write("base=1.0001\n");
            w.Write("lmscale=9.5\n");
            w.Write("start=0\n");
            w.Write("end=1\n");
            w.Write("#\n# Size line.\n#\n");
            w.Write("NODES=" + Nodes.Count + "    LINKS=" + Edges.Count + "\n");

            // we cannot use the id from sphinx as node id. The id from sphinx may be arbitrarily big.
            // Certain tools, such as lattice-tool from srilm, may elect to use an array to hold the nodes, 
            // which might cause out of memory problem due to huge array.
            var nodeIdMap = new HashMap<String, Int32>();

            nodeIdMap.Put(InitialNode.Id, 0);
            nodeIdMap.Put(TerminalNode.Id, 1);

            var count = 2;
            w.Write("#\n# Nodes definitions.\n#\n");
            foreach (var node in Nodes.Values)
            {
                if (nodeIdMap.ContainsKey(node.Id))
                {
                    w.Write("I=" + nodeIdMap.Get(node.Id));
                }
                else
                {
                    nodeIdMap.Add(node.Id, count);
                    w.Write("I=" + count);
                    count++;
                }
                w.Write("    t=" + (node.BeginTime * 1.0 / 1000));
                var spelling = node.Word.Spelling;
                if (spelling.StartsWith("<"))
                    spelling = "!NULL";
                w.Write("    W=" + spelling);
                w.Write("\n");
            }
            w.Write("#\n# Link definitions.\n#\n");
            count = 0;
            foreach (var edge in Edges)
            {
                w.Write("J=" + count);
                w.Write("    S=" + nodeIdMap.Get(edge.FromNode.Id));
                w.Write("    E=" + nodeIdMap.Get(edge.ToNode.Id));
                w.Write("    a=" + edge.AcousticScore);
                w.Write("    l=" + edge.LMScore / 9.5);
                w.Write("\n");
                count++;
            }
            w.Flush();
        }

        /**
        /// Dump the Lattice as a .LAT file
         *
        /// @param out
        /// @throws IOException
         */
        protected void Dump(StreamWriter _out)
        {
            //System.err.println( "Dumping to " + out );
            foreach (var node in Nodes.Values)
            {
                node.Dump(_out);
            }
            foreach (var edge in Edges)
            {
                edge.Dump(_out);
            }
            _out.WriteLine("initialNode: " + InitialNode.Id);
            _out.WriteLine("terminalNode: " + TerminalNode.Id);
            _out.WriteLine("logBase: " + LogMath.LogBase);
            _out.Flush();
        }


        /**
        /// Dump the Lattice as a .LAT file.  Used to save Lattices as ASCII files for testing and experimentation.
         *
        /// @param file
         */
        public void Dump(String file)
        {
            try
            {
                Dump(new StreamWriter(file));
            }
            catch (IOException e)
            {
                throw new Error(e.ToString());
            }
        }


        /**
        /// Remove a Node and all Edges connected to it.  Also remove those Edges from all connected Nodes.
         *
        /// @param n
         */
        public void RemoveNodeAndEdges(Node n)
        {
            //System.err.println("Removing node " + n + " and associated edges");
            foreach (var e in n.LeavingEdges)
            {
                e.ToNode.RemoveEnteringEdge(e);
                //System.err.println( "\tRemoving " + e );
                Edges.Remove(e);
            }
            foreach (var e in n.EnteringEdges)
            {
                e.FromNode.RemoveLeavingEdge(e);
                //System.err.println( "\tRemoving " + e );
                Edges.Remove(e);
            }
            //System.err.println( "\tRemoving " + n );
            Nodes.Remove(n.Id);

            Debug.Assert(CheckConsistency());
        }


        /**
        /// Remove a Node and cross connect all Nodes with Edges to it.
        /// <p/>
        /// For example given
        /// <p/>
        /// Nodes A, B, X, M, N Edges A-->X, B-->X, X-->M, X-->N
        /// <p/>
        /// Removing and cross connecting X would result in
        /// <p/>
        /// Nodes A, B, M, N Edges A-->M, A-->N, B-->M, B-->N
         *
        /// @param n
         */
        protected void RemoveNodeAndCrossConnectEdges(Node n)
        {
            Console.WriteLine("Removing node " + n + " and cross connecting edges");
            foreach (var ei in n.EnteringEdges)
            {
                foreach (var ej in n.LeavingEdges)
                {
                    AddEdge(ei.FromNode, ej.ToNode,
                            ei.AcousticScore, ei.LMScore);
                }
            }
            RemoveNodeAndEdges(n);

            Debug.Assert(CheckConsistency());
        }


        /**
        /// Get the initialNode for this Lattice.  This corresponds usually to the <s> symbol
         *
        /// @return the initial Node
         */

        public Node InitialNode { get; set; }


        /**
        /// Set the initialNode for this Lattice.  This corresponds usually to the <s> symbol
         *
        /// @param p_initialNode
         */


        /**
        /// Get the terminalNode for this Lattice.  This corresponds usually to the </s> symbol
         *
        /// @return the initial Node
         */

        public Node TerminalNode { get; set; }


        /**
        /// Set the terminalNode for this Lattice.  This corresponds usually to the </s> symbol
         *
        /// @param p_terminalNode
         */

        /** Dump all paths through this Lattice.  Used for debugging. */
        public void DumpAllPaths()
        {
            foreach (var path in AllPaths())
            {
                Console.WriteLine(path);
            }
        }


        /**
        /// Generate a List of all paths through this Lattice.
         *
        /// @return a lists of lists of Nodes
         */
        public LinkedList<String> AllPaths()
        {
            return AllPathsFrom("", InitialNode);
        }


        /**
        /// Internal routine used to generate all paths starting at a given node.
         *
        /// @param path
        /// @param n
        /// @return a list of lists of Nodes
         */
        protected LinkedList<String> AllPathsFrom(String path, Node n)
        {
            var p = path + ' ' + n.Word;
            var l = new LinkedList<String>();
            if (n == TerminalNode)
            {
                l.Add(p);
            }
            else
            {
                foreach (var e in n.LeavingEdges)
                {
                    l.AddAll(AllPathsFrom(p, e.ToNode));
                }
            }
            return l;
        }


        bool CheckConsistency()
        {
            foreach (var n in Nodes.Values)
            {
                foreach (var e in n.EnteringEdges)
                {
                    if (!HasEdge(e))
                    {
                        throw new Exception("Lattice has NODE with missing FROM edge: "
                                + n + ',' + e);
                    }
                }
                foreach (var e in n.LeavingEdges)
                {
                    if (!HasEdge(e))
                    {
                        throw new Exception("Lattice has NODE with missing TO edge: " +
                                n + ',' + e);
                    }
                }
            }
            foreach (var e in Edges)
            {
                if (!HasNode(e.FromNode))
                {
                    throw new Exception("Lattice has EDGE with missing FROM node: " +
                        e);
                }
                if (!HasNode(e.ToNode))
                {
                    throw new Exception("Lattice has EDGE with missing TO node: " + e);
                }
                if (!e.ToNode.HasEdgeFromNode(e.FromNode))
                {
                    throw new Exception("Lattice has EDGE with TO node with no corresponding FROM edge: " + e);
                }
                if (!e.FromNode.HasEdgeToNode(e.ToNode))
                {
                    throw new Exception("Lattice has EDGE with FROM node with no corresponding TO edge: " + e);
                }
            }
            return true;
        }


        protected void SortHelper(Node n, List<Node> sorted, HashSet<Node> visited)
        {
            if (visited.Contains(n))
            {
                return;
            }
            visited.Add(n);
            if (n == null)
            {
                throw new Exception("Node is null");
            }
            foreach (var e in n.LeavingEdges)
            {
                SortHelper(e.ToNode, sorted, visited);
            }
            sorted.Add(n);
        }


        /**
        /// Topologically sort the nodes in this lattice.
         *
        /// @return Topologically sorted list of nodes in this lattice.
         */
        public List<Node> SortNodes()
        {
            var sorted = new List<Node>(Nodes.Count);
            SortHelper(InitialNode, sorted, new HashSet<Node>());
            sorted.Reverse();
            return sorted;
        }


        /**
        /// Compute the utterance-level posterior for every node in the lattice, i.e. the probability that this node occurs
        /// on any path through the lattice. Uses a forward-backward algorithm specific to the nature of non-looping
        /// left-to-right lattice structures.
        /// <p/>
        /// Node posteriors can be retrieved by calling getPosterior() on Node objects.
         *
        /// @param languageModelWeightAdjustment the weight multiplier that will be applied to language score already scaled by language weight
         */
        public void ComputeNodePosteriors(float languageModelWeightAdjustment)
        {
            ComputeNodePosteriors(languageModelWeightAdjustment, false);
        }


        /**
        /// Compute the utterance-level posterior for every node in the lattice, i.e. the probability that this node occurs
        /// on any path through the lattice. Uses a forward-backward algorithm specific to the nature of non-looping
        /// left-to-right lattice structures.
        /// <p/>
        /// Node posteriors can be retrieved by calling getPosterior() on Node objects.
         *
        /// @param languageModelWeightAdjustment   the weight multiplier that will be applied to language score already scaled by language weight
        /// @param useAcousticScoresOnly use only the acoustic scores to compute the posteriors, ignore the language weight
        ///                              and scores
         */
        public void ComputeNodePosteriors(float languageModelWeightAdjustment, bool useAcousticScoresOnly)
        {
            if (InitialNode == null)
                return;
            //forward
            InitialNode.ForwardScore = LogMath.LogOne;
            InitialNode.ViterbiScore = LogMath.LogOne;
            var sortedNodes = SortNodes();
            Debug.Assert(sortedNodes[0] == InitialNode);
            foreach (var currentNode in sortedNodes)
            {
                foreach (var edge in currentNode.LeavingEdges)
                {
                    var forwardProb = edge.FromNode.ForwardScore;
                    var edgeScore = ComputeEdgeScore
                            (edge, languageModelWeightAdjustment, useAcousticScoresOnly);
                    forwardProb += edgeScore;
                    edge.ToNode.ForwardScore = LogMath.AddAsLinear
                        ((float)forwardProb,
                            (float)edge.ToNode.ForwardScore);
                    var vs = edge.FromNode.ViterbiScore +
                            edgeScore;
                    if (edge.ToNode.BestPredecessor == null ||
                            vs > edge.ToNode.ViterbiScore)
                    {
                        edge.ToNode.BestPredecessor = currentNode;
                        edge.ToNode.ViterbiScore = vs;
                    }
                }
            }

            //backward
            TerminalNode.BackwardScore = LogMath.LogOne;
            Debug.Assert(sortedNodes[sortedNodes.Count - 1] == TerminalNode);
            //var n = sortedNodes.GetEnumerator().ListIterator(sortedNodes.Count-1);

            var n = sortedNodes.Count;
            while (n > 0)
            {
                var currentNode = sortedNodes[--n]; //TODO: Check behavior
                var currentEdges = currentNode.LeavingEdges;
                foreach (var edge in currentEdges)
                {
                    var backwardProb = edge.ToNode.BackwardScore;
                    backwardProb += ComputeEdgeScore
                            (edge, languageModelWeightAdjustment, useAcousticScoresOnly);
                    edge.FromNode.BackwardScore = LogMath.AddAsLinear((float)backwardProb,
                        (float)edge.FromNode.BackwardScore);
                }
            }

            //inner
            var normalizationFactor = TerminalNode.ForwardScore;
            foreach (var node in Nodes.Values)
            {
                node.Posterior = (node.ForwardScore +
                                  node.BackwardScore) - normalizationFactor;
            }
        }


        /**
        /// Retrieves the MAP path from this lattice. Only works once computeNodePosteriors has been called.
         *
        /// @return a list of nodes representing the MAP path.
         */
        public LinkedList<Node> GetViterbiPath()
        {
            var path = new LinkedList<Node>();
            var n = TerminalNode;
            while (n != InitialNode)
            {
                path.AddFirst(n); //insert first
                n = n.BestPredecessor;
            }
            path.AddFirst(InitialNode);
            return path;
        }

        /**
     * Retrieves the list of WordResult from this lattice. Only works once computeNodePosteriors has been called.
     * 
     * @return list of WordResult
     */
        public LinkedList<WordResult> GetWordResultPath()
        {
            var path = GetViterbiPath();
            var wordResults = new LinkedList<WordResult>();
            foreach (Node node in path)
            {
                if (node.Word.IsSentenceStartWord || node.Word.IsSentenceEndWord)
                    continue;
                wordResults.Add(new WordResult(node));
            }
            return wordResults;
        }


        /**
        /// Computes the score of an edge. It multiplies on adjustment since language model
        /// score is already scaled by language model weight in linguist.
         *
        /// @param edge                the edge which score we want to compute
        /// @param languageModelWeightAdjustment the weight multiplier that will be applied to language score already scaled by language weight
        /// @return the score of an edge
         */
        private static double ComputeEdgeScore(Edge edge, float languageModelWeightAdjustment, Boolean useAcousticScoresOnly)
        {
            if (useAcousticScoresOnly)
            {
                return edge.AcousticScore;
            }
            else
            {
                return edge.AcousticScore + edge.LMScore * languageModelWeightAdjustment;
            }
        }


        /**
        /// Returns true if the given Lattice is equivalent to this Lattice. Two lattices are equivalent if all their nodes
        /// and edges are equivalent.
         *
        /// @param other the Lattice to compare this Lattice against
        /// @return true if the Lattices are equivalent; false otherwise
         */
        public Boolean IsEquivalent(Lattice other)
        {
            return CheckNodesEquivalent(InitialNode, other.InitialNode);
        }


        /**
        /// Returns true if the two lattices starting at the given two nodes are equivalent. It recursively checks all the
        /// child nodes until these two nodes until there are no more child nodes.
         *
        /// @param n1 starting node of the first lattice
        /// @param n2 starting node of the second lattice
        /// @return true if the two lattices are equivalent
         */
        private static bool CheckNodesEquivalent(Node n1, Node n2)
        {
            Debug.Assert(n1 != null && n2 != null);

            var equivalent = n1.IsEquivalent(n2);
            if (equivalent)
            {
                var leavingEdges = n1.GetCopyOfLeavingEdges();
                var leavingEdges2 = n2.GetCopyOfLeavingEdges();

                Console.WriteLine("# edges: " + leavingEdges.Count + " " + leavingEdges2.Count);

                foreach (var edge in leavingEdges)
                {
                    /* find an equivalent edge from n2 for this edge */
                    var e2 = n2.FindEquivalentLeavingEdge(edge);

                    if (e2 == null)
                    {
                        Console.WriteLine("Equivalent edge not found, lattices not equivalent.");
                        return false;
                    }
                    else
                    {
                        if (!leavingEdges2.Remove(e2))
                        {
                            /*
                            /// if it cannot be removed, then the leaving edges
                            /// are not the same
                             */
                            Console.WriteLine("Equivalent edge already matched, lattices not equivalent.");
                            return false;
                        }
                        else
                        {
                            /* recursively check the two child nodes */
                            equivalent &= CheckNodesEquivalent
                                    (edge.ToNode, e2.ToNode);
                            if (!equivalent)
                            {
                                return false;
                            }
                        }
                    }
                }
                if (leavingEdges2.Count != 0)
                {
                    Console.WriteLine("One lattice has too many edges.");
                    return false;
                }
            }
            return equivalent;
        }


        static Boolean IsFillerNode(Node node)
        {
            return node.Word.Spelling.Equals("<sil>");
        }


        public void RemoveFillers()
        {
            foreach (var node in SortNodes())
            {
                if (IsFillerNode(node))
                {
                    RemoveNodeAndCrossConnectEdges(node);
                    Debug.Assert(CheckConsistency());
                }
            }
        }

    }
}
