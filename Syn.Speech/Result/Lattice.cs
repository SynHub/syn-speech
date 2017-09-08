using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Syn.Speech.Common;
using Syn.Speech.Decoder.Search;
using Syn.Speech.Linguist;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;

namespace Syn.Speech.Result
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
        protected Node initialNode;
        public Node terminalNode;
        protected List<Edge> edges=new List<Edge>();
        protected Dictionary<String, Node> nodes= new Dictionary<String, Node>();
        protected double logBase;
        protected LogMath logMath=LogMath.getLogMath();
        private List<Token> visitedWordTokens;
        private AlternateHypothesisManager loserManager;


        /** Create an empty Lattice. */
        public Lattice() {
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
        {    
            visitedWordTokens = new List<Token>();
            loserManager = result.getAlternateHypothesisManager();
            if (loserManager != null) {
                loserManager.purge();
            }

            List<Token> tokens = null;
            if(result.getBestFinalToken() == null)
                tokens = result.getActiveTokens().getTokens();
            else
                tokens = result.getResultTokens();

            foreach (Token token in tokens) 
            {
                Token tokenLocal = token.getPredecessor();
                while (tokenLocal != null && !tokenLocal.isWord()) 
                {
                    tokenLocal = tokenLocal.getPredecessor();
                }
                Trace.Assert( tokenLocal != null && tokenLocal.getWord().isSentenceEndWord());
                if (terminalNode == null) 
                {
                    terminalNode = new Node(getNodeID(result.getBestToken()),
                            token.getWord(), -1, -1);
                    initialNode = terminalNode; 
                    addNode(terminalNode);
                }
                collapseWordToken(tokenLocal);
            }
        }


        /**
        /// Returns the node corresponding to the given word token.
         *
        /// @param token the token which we want a node of
        /// @return the node of the given token
         */
        private Node getNode(Token token) 
        {
            if (token.getWord().isSentenceEndWord()) 
            {
                return terminalNode;
            }
            Node node = nodes[getNodeID(token)];
            if (node == null) 
            {
                IWordSearchState wordState =
                        (IWordSearchState) token.getSearchState();

                int startFrame = -1;
                int endFrame = -1;

                if (wordState.isWordStart()) {
                    startFrame = token.getFrameNumber();
                } else {
                    endFrame = token.getFrameNumber();
                }

                node = new Node(getNodeID(token), token.getWord(),
                        startFrame, endFrame);
                addNode(node);
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
        private void collapseWordToken(Token token) 
        {
            Trace.Assert(token != null);
            if (visitedWordTokens.Contains(token)) 
            {
                return;
            }
            visitedWordTokens.Add(token);

            collapseWordPath(getNode(token), token.getPredecessor(),
                    token.getAcousticScore() + token.getInsertionScore(),
                    token.getLanguageScore());

            if (loserManager != null
                    && loserManager.hasAlternatePredecessors(token)) {
                foreach (Token loser in loserManager.getAlternatePredecessors(token)) 
                {
                    collapseWordPath(getNode(token), loser,
                            token.getAcousticScore(), token.getLanguageScore());
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
        private void collapseWordPath(Node parentWordNode, Token token,
                float acousticScore, float languageScore) {

            if (token == null)
                return;

            if (token.isWord()) {
                /*
                /// If this is a word, create a Node for it, and then create an edge
                /// from the Node to the parentWordNode
                 */
                Node fromNode = getNode(token);
                addEdge(fromNode, parentWordNode, acousticScore,
                        languageScore);
                if (token.getPredecessor() != null) {
                    /* Collapse the token sequence ending in this token. */
                    collapseWordToken(token);
                } else {
                    /* we've reached the sentence start token */
                    Trace.Assert(token.getWord().isSentenceStartWord());
                    initialNode = fromNode;
                }
                return;
            }

            /*
            /// If a non-word token, just add the acoustic and language scores to the
            /// current totals, and then move on to the predecessor token. Fast
            /// forward through the not so interesting states to save stack space.
             */
            while (true) {
                acousticScore += token.getAcousticScore()
                        + token.getInsertionScore();
                languageScore += token.getLanguageScore();
                Token preToken = token.getPredecessor();

                if (preToken == null)
                    return;

                if (preToken.isWord()
                        || (loserManager != null && loserManager
                                .hasAlternatePredecessors(token)))
                    break;
                token = preToken;
            }

            collapseWordPath(parentWordNode, token.getPredecessor(), acousticScore,
                    languageScore);

            /* Traverse the path(s) for the loser token(s). */
            if (loserManager != null
                    && loserManager.hasAlternatePredecessors(token)) {
                foreach (Token loser in loserManager.getAlternatePredecessors(token)) 
                {
                    collapseWordPath(parentWordNode, loser, acousticScore,
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
        private String getNodeID(Token token) 
        {
            return token.GetHashCode().ToString();
        }


        /**
        /// Create a Lattice from a LAT file.  LAT files are created by the method Lattice.dump()
         *
        /// @param fileName
         */
        public Lattice(String fileName) 
        {
            try {
                Debug.Print("Loading from " + fileName);

                // load the nodes
                LineNumberReader _in = new LineNumberReader(new StreamReader(fileName));
                String line=_in.readLine();
                while (line != null) 
                {
                    StringTokenizer tokens = new StringTokenizer(line);
                    
                    if (tokens.hasMoreTokens()) {
                        String type = tokens.nextToken();

                        if (type.Equals("edge:")) 
                        {
                            Edge.load(this, tokens);
                        } 
                        else if (type.Equals("node:")) 
                        {
                            Node.load(this, tokens);
                        } 
                        else if (type.Equals("initialNode:")) 
                        {
                            setInitialNode(getNode(tokens.nextToken()));
                        } 
                        else if (type.Equals("terminalNode:")) 
                        {
                            setTerminalNode(getNode(tokens.nextToken()));
                        } 
                        else if (type.Equals("logBase:")) 
                        {
                            logBase = Double.Parse(tokens.nextToken());
                        } 
                        else 
                        {
                    	    _in.close();
                            throw new Exception("SYNTAX ERROR: " + fileName +
                                '[' + _in.getLineNumber() + "] " + line);
                        }
                    }
                    line=_in.readLine();
                }
                _in.close();
            } 
            catch (Exception e) {
                throw new Exception(e.ToString());
            }
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
        public Edge addEdge(Node fromNode, Node toNode,
                            double acousticScore, double lmScore) 
        {
            Edge e = new Edge(fromNode, toNode, acousticScore, lmScore);
            fromNode.addLeavingEdge(e);
            toNode.addEnteringEdge(e);
            edges.Add(e);
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
        public Node addNode(IWord word, int beginTime, int endTime) 
        {
            Node n = new Node(word, beginTime, endTime);
            addNode(n);
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
        protected Node addNode(String id, IWord word, int beginTime, int endTime) 
        {
            Node n = new Node(id, word, beginTime, endTime);
            addNode(n);
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
        public Node addNode(String id, String word, int beginTime, int endTime) 
        {
            Word w = new Word(word, new Pronunciation[0], false);
            return addNode(id, w, beginTime, endTime);
        }


        /**
        /// Add a Node corresponding to a Token from the result Token tree. Usually, the Token should reference a search
        /// state that is a WordSearchState, although other Tokens may be used for debugging.
         *
        /// @param token
        /// @return the new Node
         */
        protected Node addNode(Token token, int beginTime, int endTime) 
        {
            Trace.Assert(token.getSearchState() is IWordSearchState);
            IWord word = ((IWordSearchState) (token.getSearchState()))
                    .getPronunciation().getWord();
            return addNode(token.GetHashCode().ToString(), word, beginTime, endTime);
        }


        /**
        /// Test to see if the Lattice contains an Edge
         *
        /// @param edge
        /// @return true if yes
         */
        public Boolean hasEdge(Edge edge) 
        {
            return edges.Contains(edge);
        }


        /**
        /// Test to see if the Lattice contains a Node
         *
        /// @param node
        /// @return true if yes
         */
        public Boolean hasNode(Node node) 
        {
            return hasNode(node.getId());
        }

    
        /**
        /// Test to see if the Lattice already contains a Node corresponding to a given Token.
         *
        /// @param ID the ID of the Node to find
        /// @return true if yes
         */
        public Boolean hasNode(String ID) 
        {
            return nodes.ContainsKey(ID);
        }


        /**
        /// Add a Node to the set of all Nodes
         *
        /// @param n
         */
        protected void addNode(Node n) 
        {
            Trace.Assert(!hasNode(n.getId()));
            nodes.Add(n.getId(), n);
        }


        /**
        /// Remove a Node from the set of all Nodes
         *
        /// @param n
         */
        protected void removeNode(Node n) 
        {
            Trace.Assert(hasNode(n.getId()));
            nodes.Remove(n.getId());
        }


        /**
        /// Get the Node associated with an ID
         *
        /// @param id
        /// @return the Node
         */
        public  Node getNode(String id) 
        {
            return (nodes[id]);
        }


        /**
        /// Get a copy of the Collection of all Nodes. Used by LatticeOptimizer to avoid Concurrent modification of the nodes
        /// list.
         *
        /// @return a copy of the collection of Nodes
         */
        public List<Node> getCopyOfNodes() 
        {
            return nodes.Values.ToList();
        }


        /**
        /// Get the Collection of all Nodes.
         *
        /// @return the collection of all Nodes
         */
        public List<Node> getNodes() 
        {
            return nodes.Values.ToList();
        }


        /**
        /// Remove an Edge from the set of all Edges.
         *
        /// @param e
         */
        protected void removeEdge(Edge e) 
        {
            edges.Remove(e);
        }


        /**
        /// Get the set of all Edges.
         *
        /// @return the set of all edges
         */
        public List<Edge> getEdges() 
        {
            return edges;
        }


        /**
        /// Dump the Lattice in the form understood by AiSee (a graph visualization tool).  See http://www.AbsInt.com
         *
        /// @param fileName
        /// @param title
         */
        public void dumpAISee(String fileName, String title) 
        {
            try {
                Debug.Print("Dumping " + title + " to " + fileName);
                StreamWriter f = new StreamWriter(fileName);
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

                foreach (Node node in nodes.Values) 
                {
                    node.dumpAISee(f);
                }
                foreach (Edge edge in edges) 
                {
                    edge.dumpAISee(f);
                }
                f.Write("}\n");
                f.Close();
            } 
            catch (IOException e) 
            {
                throw e;
            }
        }

        /**
        /// Dump the Lattice in the form understood by Graphviz. See http://graphviz.org
         *
        /// @param fileName
        /// @param title
         */
        public void dumpDot(String fileName, String title) 
        {
            try {
                Debug.Print("Dumping " + title + " to " + fileName);
                StreamWriter f = new StreamWriter(fileName);
                f.Write("digraph \"" + title + "\" {\n");
                f.Write("rankdir = LR\n");

                foreach (Node node in nodes.Values) 
                {
                    node.dumpDot(f);
                }
                foreach (Edge edge in edges) 
                {
                    edge.dumpDot(f);
                }
                f.Write("}\n");
                f.Close();
            } 
            catch (IOException e) {
                throw e;
            }
        }


        public void dumpSlf(StreamWriter w)
        {
             w.Write("VERSION=1.1\n"); 
             w.Write("UTTERANCE=test\n");
             w.Write("base=1.0001\n");
             w.Write("lmscale=9.5\n");
             w.Write("start=0\n");
             w.Write("end=1\n");
             w.Write("#\n# Size line.\n#\n");
             w.Write("NODES="+nodes.Count.ToString()+"    LINKS="+this.edges.Count.ToString()+"\n");

             // we cannot use the id from sphinx as node id. The id from sphinx may be arbitrarily big.
             // Certain tools, such as lattice-tool from srilm, may elect to use an array to hold the nodes, 
             // which might cause out of memory problem due to huge array.
             Dictionary<String, Int32> nodeIdMap=new Dictionary<String, Int32>();

             nodeIdMap.Add(initialNode.getId(), 0);
             nodeIdMap.Add(terminalNode.getId(), 1);

             int count=2;
             w.Write("#\n# Nodes definitions.\n#\n");
             foreach(Node node in nodes.Values)
             {
                  if (nodeIdMap.ContainsKey(node.getId())) 
                  {
                      w.Write("I=" + nodeIdMap[node.getId()]);
                  } 
                  else 
                  {
                      nodeIdMap.Add(node.getId(), count);
                       w.Write("I=" + count);
                      count++;
                  }
                  w.Write("    t="+(node.getBeginTime()*1.0/1000));
                  String spelling = node.getWord().getSpelling();
                  if (spelling.StartsWith("<"))
            	        spelling = "!NULL";
                  w.Write("    W=" + spelling);
                  w.Write("\n");
             }
             w.Write("#\n# Link definitions.\n#\n");
             count=0;
             foreach(Edge edge in edges)
            {
                  w.Write("J="+count);
                  w.Write("    S="+nodeIdMap[edge.getFromNode().getId()]);
                  w.Write("    E="+nodeIdMap[edge.getToNode().getId()]);
                  w.Write("    a="+edge.getAcousticScore());
                  w.Write("    l="+edge.getLMScore() / 9.5);
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
        protected void dump(StreamWriter _out)
        {
            //System.err.println( "Dumping to " + out );
            foreach (Node node in nodes.Values) 
            {
                node.dump(_out);
            }
            foreach (Edge edge in edges) 
            {
                edge.dump(_out);
            }
            _out.WriteLine("initialNode: " + initialNode.getId());
            _out.WriteLine("terminalNode: " + terminalNode.getId());
            _out.WriteLine("logBase: " + logMath.getLogBase());
            _out.Flush();
        }


        /**
        /// Dump the Lattice as a .LAT file.  Used to save Lattices as ASCII files for testing and experimentation.
         *
        /// @param file
         */
        public void dump(String file) 
        {
            try 
            {
                dump(new StreamWriter(file));
            } 
            catch (IOException e) 
            {
                throw e;
            }
        }


        /**
        /// Remove a Node and all Edges connected to it.  Also remove those Edges from all connected Nodes.
         *
        /// @param n
         */
        public void removeNodeAndEdges(Node n) 
        {
            //System.err.println("Removing node " + n + " and associated edges");
            foreach (Edge e in n.getLeavingEdges()) 
            {
                e.getToNode().removeEnteringEdge(e);
                //System.err.println( "\tRemoving " + e );
                edges.Remove(e);
            }
            foreach (Edge e in n.getEnteringEdges()) 
            {
                e.getFromNode().removeLeavingEdge(e);
                //System.err.println( "\tRemoving " + e );
                edges.Remove(e);
            }
            //System.err.println( "\tRemoving " + n );
            nodes.Remove(n.getId());

            Trace.Assert(checkConsistency());
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
        protected void removeNodeAndCrossConnectEdges(Node n) 
        {
            Debug.WriteLine("Removing node " + n + " and cross connecting edges");
            foreach (Edge ei in n.getEnteringEdges()) 
            {
                foreach (Edge ej in n.getLeavingEdges()) 
                {
                    addEdge(ei.getFromNode(), ej.getToNode(),
                            ei.getAcousticScore(), ei.getLMScore());
                }
            }
            removeNodeAndEdges(n);

            Trace.Assert(checkConsistency());
        }


        /**
        /// Get the initialNode for this Lattice.  This corresponds usually to the <s> symbol
         *
        /// @return the initial Node
         */
        public Node getInitialNode() 
        {
            return initialNode;
        }


        /**
        /// Set the initialNode for this Lattice.  This corresponds usually to the <s> symbol
         *
        /// @param p_initialNode
         */
        public void setInitialNode(Node p_initialNode) 
        {
            initialNode = p_initialNode;
        }


        /**
        /// Get the terminalNode for this Lattice.  This corresponds usually to the </s> symbol
         *
        /// @return the initial Node
         */
        public Node getTerminalNode() 
        {
            return terminalNode;
        }


        /**
        /// Set the terminalNode for this Lattice.  This corresponds usually to the </s> symbol
         *
        /// @param p_terminalNode
         */
        public void setTerminalNode(Node p_terminalNode) 
        {
            terminalNode = p_terminalNode;
        }

        /** Dump all paths through this Lattice.  Used for debugging. */
        public void dumpAllPaths() 
        {
            foreach (String path in allPaths()) 
            {
                Debug.WriteLine(path);
            }
        }


        /**
        /// Generate a List of all paths through this Lattice.
         *
        /// @return a lists of lists of Nodes
         */
        public List<String> allPaths() 
        {
            return allPathsFrom("", initialNode);
        }


        /**
        /// Internal routine used to generate all paths starting at a given node.
         *
        /// @param path
        /// @param n
        /// @return a list of lists of Nodes
         */
        protected List<String> allPathsFrom(String path, Node n) 
        {
            String p = path + ' ' + n.getWord();
            List<String> l = new List<String>();
            if (n == terminalNode) 
            {
                l.Add(p);
            } 
            else 
            {
                foreach (Edge e in n.getLeavingEdges()) 
                {
                    l.AddRange(allPathsFrom(p, e.getToNode()));
                }
            }
            return l;
        }


        Boolean checkConsistency() 
        {
            foreach (Node n in nodes.Values) 
            {
                foreach (Edge e in n.getEnteringEdges()) 
                {
                    if (!hasEdge(e)) 
                    {
                        throw new Exception("Lattice has NODE with missing FROM edge: "
                                + n + ',' + e);
                    }
                }
                foreach (Edge e in n.getLeavingEdges()) 
                {
                    if (!hasEdge(e)) 
                    {
                        throw new Exception("Lattice has NODE with missing TO edge: " +
                                n + ',' + e);
                    }
                }
            }
            foreach (Edge e in edges) 
            {
                if (!hasNode(e.getFromNode())) 
                {
                    throw new Exception("Lattice has EDGE with missing FROM node: " +
                        e);
                }
                if (!hasNode(e.getToNode())) 
                {
                    throw new Exception("Lattice has EDGE with missing TO node: " + e);
                }
                if (!e.getToNode().hasEdgeFromNode(e.getFromNode())) 
                {
                    throw new Exception("Lattice has EDGE with TO node with no corresponding FROM edge: " + e);
                }
                if (!e.getFromNode().hasEdgeToNode(e.getToNode())) 
                {
                    throw new Exception("Lattice has EDGE with FROM node with no corresponding TO edge: " + e);
                }
            }
            return true;
        }


        protected void sortHelper(Node n, List<Node> sorted, List<Node> visited) 
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
            foreach (Edge e in n.getLeavingEdges()) 
            {
                sortHelper(e.getToNode(), sorted, visited);
            }
            sorted.Add(n);
        }


        /**
        /// Topologically sort the nodes in this lattice.
         *
        /// @return Topologically sorted list of nodes in this lattice.
         */
        public List<Node> sortNodes() 
        {
            List<Node> sorted = new List<Node>(nodes.Count);
            sortHelper(initialNode, sorted, new List<Node>());
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
        public void computeNodePosteriors(float languageModelWeightAdjustment) 
        {
            computeNodePosteriors(languageModelWeightAdjustment, false);
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
        public void computeNodePosteriors(float languageModelWeightAdjustment,
                                          Boolean useAcousticScoresOnly) 
        {
            if (initialNode == null)
                    return;
            //forward
            initialNode.setForwardScore(LogMath.LOG_ONE);
            initialNode.setViterbiScore(LogMath.LOG_ONE);
            List<Node> sortedNodes = sortNodes();
            Trace.Assert(sortedNodes[0] == initialNode);
            foreach (Node currentNode in sortedNodes) 
            {
                foreach (Edge edge in currentNode.getLeavingEdges()) 
                {
                    double forwardProb = edge.getFromNode().getForwardScore();
                    double edgeScore = computeEdgeScore
                            (edge, languageModelWeightAdjustment, useAcousticScoresOnly);
                    forwardProb += edgeScore;
                    edge.getToNode().setForwardScore
                            (logMath.addAsLinear
                                    ((float) forwardProb,
                                            (float) edge.getToNode().getForwardScore()));
                    double vs = edge.getFromNode().getViterbiScore() +
                            edgeScore;
                    if (edge.getToNode().getBestPredecessor() == null ||
                            vs > edge.getToNode().getViterbiScore()) {
                        edge.getToNode().setBestPredecessor(currentNode);
                        edge.getToNode().setViterbiScore(vs);
                    }
                }
            }

            //backward
            terminalNode.setBackwardScore(LogMath.LOG_ONE);
            Trace.Assert(sortedNodes[sortedNodes.Count - 1] == terminalNode);

            int n = sortedNodes.Count - 1;
            while (n > 0) 
            {
                Node currentNode = sortedNodes[n-1];
                List<Edge> currentEdges = currentNode.getLeavingEdges();
                foreach (Edge edge in currentEdges) 
                {
                    double backwardProb = edge.getToNode().getBackwardScore();
                    backwardProb += computeEdgeScore
                            (edge, languageModelWeightAdjustment, useAcousticScoresOnly);
                    edge.getFromNode().setBackwardScore
                            (logMath.addAsLinear((float) backwardProb,
                                    (float) edge.getFromNode().getBackwardScore()));
                }
            }

            //inner
            double normalizationFactor = terminalNode.getForwardScore();
            foreach (Node node in nodes.Values) 
            {
                node.setPosterior((node.getForwardScore() +
                    node.getBackwardScore()) - normalizationFactor);
            }
        }


        /**
        /// Retrieves the MAP path from this lattice. Only works once computeNodePosteriors has been called.
         *
        /// @return a list of nodes representing the MAP path.
         */
        public List<Node> getViterbiPath() 
        {
            List<Node> path = new List<Node>();
            Node n = terminalNode;
            while (n != initialNode) 
            {
                path.Insert(0,n); //insert first
                n = n.getBestPredecessor();
            }
            path.Insert(0,initialNode);
            return path;
        }


        /**
        /// Computes the score of an edge. It multiplies on adjustment since language model
        /// score is already scaled by language model weight in linguist.
         *
        /// @param edge                the edge which score we want to compute
        /// @param languageModelWeightAdjustment the weight multiplier that will be applied to language score already scaled by language weight
        /// @return the score of an edge
         */
        private double computeEdgeScore(Edge edge, float languageModelWeightAdjustment,
                                        Boolean useAcousticScoresOnly) {
            if (useAcousticScoresOnly) 
            {
                return edge.getAcousticScore();
            } 
            else 
            {
                return edge.getAcousticScore() + edge.getLMScore()* languageModelWeightAdjustment;
            }
        }


        /**
        /// Returns true if the given Lattice is equivalent to this Lattice. Two lattices are equivalent if all their nodes
        /// and edges are equivalent.
         *
        /// @param other the Lattice to compare this Lattice against
        /// @return true if the Lattices are equivalent; false otherwise
         */
        public Boolean isEquivalent(Lattice other) 
        {
            return checkNodesEquivalent(initialNode, other.getInitialNode());
        }


        /**
        /// Returns true if the two lattices starting at the given two nodes are equivalent. It recursively checks all the
        /// child nodes until these two nodes until there are no more child nodes.
         *
        /// @param n1 starting node of the first lattice
        /// @param n2 starting node of the second lattice
        /// @return true if the two lattices are equivalent
         */
        private Boolean checkNodesEquivalent(Node n1, Node n2) 
        {
            Trace.Assert(n1 != null && n2 != null);

            Boolean equivalent = n1.isEquivalent(n2);
            if (equivalent) {
                List<Edge> leavingEdges = n1.getCopyOfLeavingEdges();
                List<Edge> leavingEdges2 = n2.getCopyOfLeavingEdges();

                Debug.WriteLine("# edges: " + leavingEdges.Count.ToString() + " "+
                        leavingEdges2.Count.ToString());

                foreach (Edge edge in leavingEdges) 
                {
            	    /* find an equivalent edge from n2 for this edge */
                    Edge e2 = n2.findEquivalentLeavingEdge(edge);

                    if (e2 == null) 
                    {
                        Debug.WriteLine("Equivalent edge not found, lattices not equivalent.");
                        return false;
                    } else {
                        if (!leavingEdges2.Remove(e2)) 
                        {
                            /*
                            /// if it cannot be removed, then the leaving edges
                            /// are not the same
                             */
                            Debug.WriteLine("Equivalent edge already matched, lattices not equivalent.");
                            return false;
                        } else {
                            /* recursively check the two child nodes */
                            equivalent &= checkNodesEquivalent
                                    (edge.getToNode(), e2.getToNode());
                            if (!equivalent) {
                                return false;
                            }
                        }
                    }
                }
                if (leavingEdges2.Count!=0) 
                {
                    Debug.WriteLine("One lattice has too many edges.");
                    return false;
                }
            }
            return equivalent;
        }
    

        Boolean isFillerNode(Node node) 
        {
            return node.getWord().getSpelling().Equals("<sil>");
        }

    
        public void removeFillers() 
        {
            foreach (Node node in sortNodes()) 
            {
                if (isFillerNode(node)) 
                {
                    removeNodeAndCrossConnectEdges(node);
                    Trace.Assert(checkConsistency());
                }
            }
        }

    }
}
