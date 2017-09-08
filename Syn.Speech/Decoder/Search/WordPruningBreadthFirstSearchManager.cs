using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Syn.Speech.Common.FrontEnd;
using Syn.Speech.Decoder.Pruner;
using Syn.Speech.Decoder.Scorer;
using Syn.Speech.Linguist;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// Provides the breadth first search. To perform recognition an application should call initialize before recognition
    /// begins, and repeatedly call <code> recognize </code> until Result.isFinal() returns true. Once a final result has
    /// been obtained, <code> stopRecognition </code> should be called.
    ///
    /// All scores and probabilities are maintained in the log math log domain.
    /// </summary>
    public class WordPruningBreadthFirstSearchManager : TokenSearchManager
    {
        /** The property that defines the name of the linguist to be used by this search manager. */
        [S4Component(type = typeof(Linguist.Linguist))]
        public static String PROP_LINGUIST = "linguist";

        /** The property that defines the name of the linguist to be used by this search manager. */
        [S4Component(type = typeof(IPruner))]
        public static String PROP_PRUNER = "pruner";

        /** The property that defines the name of the scorer to be used by this search manager. */
        [S4Component(type = typeof(IAcousticScorer))]
        public static String PROP_SCORER = "scorer";

        /**
        /// The property than, when set to <code>true</code> will cause the recognizer to count up all the tokens in the
        /// active list after every frame.
         */
        [S4Boolean(defaultValue = false)]
        public static String PROP_SHOW_TOKEN_COUNT = "showTokenCount";

        /**
        /// The property that controls the number of frames processed for every time
        /// the decode growth step is skipped. Setting this property to zero disables
        /// grow skipping. Setting this number to a small integer will increase the
        /// speed of the decoder but will also decrease its accuracy. The higher the
        /// number, the less often the grow code is skipped. Values like 6-8 is known
        /// to be the good enough for large vocabulary tasks. That means that one of
        /// 6 frames will be skipped.
         */
        [S4Integer(defaultValue = 0)]
        public static String PROP_GROW_SKIP_INTERVAL = "growSkipInterval";

        /** The property that defines the type of active list to use */
        [S4Component(type = typeof(ActiveListManager))]
        public static String PROP_ACTIVE_LIST_MANAGER = "activeListManager";

        /** The property for checking if the order of states is valid. */
        [S4Boolean(defaultValue = false)]
        public static String PROP_CHECK_STATE_ORDER = "checkStateOrder";

        /** The property that specifies the maximum lattice edges */
        [S4Integer(defaultValue = 100)]
        public static String PROP_MAX_LATTICE_EDGES = "maxLatticeEdges";

        /**
        /// The property that controls the amount of simple acoustic lookahead performed. Setting the property to zero
        /// (the default) disables simple acoustic lookahead. The lookahead need not be an integer.
         */
        [S4Double(defaultValue = 0)]
        public static String PROP_ACOUSTIC_LOOKAHEAD_FRAMES = "acousticLookaheadFrames";

        /** The property that specifies the relative beam width */
        [S4Double(defaultValue = 0.0)]
        // TODO: this should be a more meaningful default e.g. the common 1E-80
        public static String PROP_RELATIVE_BEAM_WIDTH = "relativeBeamWidth";

        // -----------------------------------
        // Configured Subcomponents
        // -----------------------------------
        private Linguist.Linguist linguist; // Provides grammar/language info
        private IPruner pruner; // used to prune the active list
        private IAcousticScorer scorer; // used to score the active list
        private ActiveListManager activeListManager;
        private LogMath logMath;

        // -----------------------------------
        // Configuration data
        // -----------------------------------

        private Boolean _showTokenCount;
        private Boolean _checkStateOrder;
        private int growSkipInterval;
        private float relativeBeamWidth;
        private float acousticLookaheadFrames;
        private int maxLatticeEdges = 100;

        // -----------------------------------
        // Instrumentation
        // -----------------------------------
        private Timer scoreTimer;
        private Timer pruneTimer;
        private Timer growTimer;
        private StatisticsVariable totalTokensScored;
        private StatisticsVariable curTokensScored;
        private StatisticsVariable tokensCreated;
        private long tokenSum;
        private int tokenCount;

        // -----------------------------------
        // Working data
        // -----------------------------------
        private int currentFrameNumber; // the current frame number
        protected ActiveList activeList; // the list of active tokens
        private List<Token> resultList; // the current set of results
        protected Dictionary<Object, Token> bestTokenMap;
        private AlternateHypothesisManager loserManager;
        private int numStateOrder;
        // private TokenTracker tokenTracker;
        // private TokenTypeTracker tokenTypeTracker;
        private Boolean streamEnd;

        /**
        /// 
        /// @param linguist
        /// @param pruner
        /// @param scorer
        /// @param activeListManager
        /// @param showTokenCount
        /// @param relativeWordBeamWidth
        /// @param growSkipInterval
        /// @param checkStateOrder
        /// @param buildWordLattice
        /// @param maxLatticeEdges
        /// @param acousticLookaheadFrames
        /// @param keepAllTokens
         */
        public WordPruningBreadthFirstSearchManager(Linguist.Linguist linguist, IPruner pruner,
                                               IAcousticScorer scorer, ActiveListManager activeListManager,
                                               Boolean showTokenCount, double relativeWordBeamWidth,
                                               int growSkipInterval,
                                               Boolean checkStateOrder, Boolean buildWordLattice,
                                               int maxLatticeEdges, float acousticLookaheadFrames,
                                               Boolean keepAllTokens)
        {

            this.logMath = LogMath.getLogMath();
            this.linguist = linguist;
            this.pruner = pruner;
            this.scorer = scorer;
            this.activeListManager = activeListManager;
            this._showTokenCount = showTokenCount;
            this.growSkipInterval = growSkipInterval;
            this._checkStateOrder = checkStateOrder;
            this.buildWordLattice = buildWordLattice;
            this.maxLatticeEdges = maxLatticeEdges;
            this.acousticLookaheadFrames = acousticLookaheadFrames;
            this.keepAllTokens = keepAllTokens;

            this.relativeBeamWidth = logMath.linearToLog(relativeWordBeamWidth);
        }

        public WordPruningBreadthFirstSearchManager()
        {

        }

        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */

        public override void newProperties(PropertySheet ps)
        {
            logMath = LogMath.getLogMath();

            linguist = (Linguist.Linguist)ps.getComponent(PROP_LINGUIST);
            pruner = (IPruner)ps.getComponent(PROP_PRUNER);
            scorer = (IAcousticScorer)ps.getComponent(PROP_SCORER);
            activeListManager = (ActiveListManager)ps.getComponent(PROP_ACTIVE_LIST_MANAGER);
            _showTokenCount = ps.getBoolean(PROP_SHOW_TOKEN_COUNT);
            growSkipInterval = ps.getInt(PROP_GROW_SKIP_INTERVAL);

            _checkStateOrder = ps.getBoolean(PROP_CHECK_STATE_ORDER);
            maxLatticeEdges = ps.getInt(PROP_MAX_LATTICE_EDGES);
            acousticLookaheadFrames = ps.getFloat(PROP_ACOUSTIC_LOOKAHEAD_FRAMES);

            relativeBeamWidth = logMath.linearToLog(ps.getDouble(PROP_RELATIVE_BEAM_WIDTH));
        }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.decoder.search.SearchManager#allocate()
        */
        override public void allocate()
        {
            // tokenTracker = new TokenTracker();
            // tokenTypeTracker = new TokenTypeTracker();

            scoreTimer = TimerPool.getTimer(this, "Score");
            pruneTimer = TimerPool.getTimer(this, "Prune");
            growTimer = TimerPool.getTimer(this, "Grow");

            totalTokensScored = StatisticsVariable.getStatisticsVariable("totalTokensScored");
            curTokensScored = StatisticsVariable.getStatisticsVariable("curTokensScored");
            tokensCreated = StatisticsVariable.getStatisticsVariable("tokensCreated");

            try
            {
                linguist.allocate();
                pruner.allocate();
                scorer.allocate();
            }
            catch (IOException e)
            {
                throw new SystemException("Allocation of search manager resources failed", e);
            }
        }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.decoder.search.SearchManager#deallocate()
        */
        public void deallocate()
        {
            try
            {
                scorer.deallocate();
                pruner.deallocate();
                linguist.deallocate();
            }
            catch (IOException e)
            {
                throw new SystemException("Deallocation of search manager resources failed", e);
            }
        }


        /// <summary>
        /// Called at the start of recognition. Gets the search manager ready to recognize
        /// </summary>
        override public void startRecognition()
        {
            linguist.startRecognition();
            pruner.startRecognition();
            scorer.startRecognition();
            localStart();
        }


        /**
        /// Performs the recognition for the given number of frames.
         *
        /// @param nFrames the number of frames to recognize
        /// @return the current result
         */
        override public Results.Result recognize(int nFrames)
        {
            Boolean done = false;
            Results.Result result = null;
            streamEnd = false;

            for (int i = 0; i < nFrames && !done; i++)
            {
                done = recognize();
            }

            if (!streamEnd)
            {
                result = new Results.Result(loserManager, activeList,
                                    resultList, currentFrameNumber, done, linguist.getSearchGraph().getWordTokenFirst());

            }

            // tokenTypeTracker.show();
            if (_showTokenCount)
            {
                showTokenCount();
            }
            return result;
        }

        private Boolean recognize()
        {

            activeList = activeListManager.getEmittingList();
            Boolean more = scoreTokens();

            if (more)
            {
                pruneBranches();
                currentFrameNumber++;
                if (growSkipInterval == 0 || (currentFrameNumber % growSkipInterval) != 0)
                {
                    clearCollectors();
                    growEmittingBranches();
                    growNonEmittingBranches();
                }
            }
            return !more;
        }

        /**
        /// Clears lists and maps before next expansion stage
         */
        private void clearCollectors()
        {
            resultList = new List<Token>();
            createBestTokenMap();
            activeListManager.clearEmittingList();
        }


        /**
        /// creates a new best token map with the best size
         */
        protected void createBestTokenMap()
        {
            int mapSize = activeList.size() * 10;
            if (mapSize == 0)
            {
                mapSize = 1;
            }
            bestTokenMap = new Dictionary<Object, Token>(mapSize);
            foreach (KeyValuePair<Object, Token> iter in bestTokenMap)
                iter.Value.setScore(0.3f);

        }


        /** Terminates a recognition */
        override public void stopRecognition()
        {
            localStop();
            scorer.stopRecognition();
            pruner.stopRecognition();
            linguist.stopRecognition();
        }


        /** Gets the initial grammar node from the linguist and creates a GrammarNodeToken */
        protected void localStart()
        {
            ISearchGraph searchGraph = linguist.getSearchGraph();
            currentFrameNumber = 0;
            curTokensScored.value = 0;
            numStateOrder = searchGraph.getNumStateOrder();
            activeListManager.setNumStateOrder(numStateOrder);
            if (buildWordLattice)
            {
                loserManager = new AlternateHypothesisManager(maxLatticeEdges);
            }

            ISearchState state = searchGraph.getInitialState();

            activeList = activeListManager.getEmittingList();
            activeList.add(new Token(state, currentFrameNumber));

            clearCollectors();

            growBranches();
            growNonEmittingBranches();
            // tokenTracker.setEnabled(false);
            // tokenTracker.startUtterance();
        }


        /** Local cleanup for this search manager */
        protected void localStop()
        {
            // tokenTracker.stopUtterance();
        }


        /**
        /// Goes through the active list of tokens and expands each token, finding the set of successor tokens until all the
        /// successor tokens are emitting tokens.
         */
        protected void growBranches()
        {
            growTimer.start();
            float relativeBeamThreshold = activeList.getBeamThreshold();

            Trace.WriteLine("Frame: " + currentFrameNumber
                        + " thresh : " + relativeBeamThreshold + " bs "
                        + activeList.getBestScore() + " tok "
                        + activeList.getBestToken());
            foreach (Token token in activeList.getTokens())
            {
                if (token.getScore() >= relativeBeamThreshold && allowExpansion(token))
                {
                    collectSuccessorTokens(token);
                }
            }
            growTimer.stop();
        }


        /**
        /// Grows the emitting branches. This version applies a simple acoustic lookahead based upon the rate of change in
        /// the current acoustic score.
         */
        protected void growEmittingBranches()
        {
            if (acousticLookaheadFrames > 0F)
            {
                growTimer.start();
                float bestScore = -float.MaxValue;
                foreach (Token t in activeList.getTokens())
                {
                    float score = t.getScore() + t.getAcousticScore()
                           * acousticLookaheadFrames;
                    if (score > bestScore)
                    {
                        bestScore = score;
                    }
                    t.setWorkingScore(score);
                }
                float relativeBeamThreshold = bestScore + relativeBeamWidth;

                foreach (Token t in activeList.getTokens())
                {
                    if (t.getWorkingScore() >= relativeBeamThreshold)
                    {
                        collectSuccessorTokens(t);
                    }
                }
                growTimer.stop();
            }
            else
            {
                growBranches();
            }
        }


        /** Grow the non-emitting branches, until the tokens reach an emitting state. */
        private void growNonEmittingBranches()
        {
            for (IEnumerator<ActiveList> i = activeListManager.getNonEmittingListIterator(); i.MoveNext(); )
            {
                activeList = i.Current;
                if (activeList != null)
                {
                    // TO DO: look for new implementation
                    //i.Current.Remove()
                    pruneBranches();
                    growBranches();
                }
            }
        }


        /**
        /// Calculate the acoustic scores for the active list. The active list should contain only emitting tokens.
         *
        /// @return <code>true</code> if there are more frames to score, otherwise, false
         */
        protected Boolean scoreTokens()
        {
            Boolean moreTokens;
            scoreTimer.start();
            IData data = scorer.calculateScores(activeList.getTokens().ConvertAll(x => (IScoreable)x));
            scoreTimer.stop();

            Token bestToken = null;
            if (data is Token)
            {
                bestToken = (Token)data;
            }
            else if (data == null)
            {
                streamEnd = true;
            }

            moreTokens = (bestToken != null);
            activeList.setBestToken(bestToken);

            //monitorWords(activeList);
            monitorStates(activeList);

            // System.out.println("BEST " + bestToken);

            curTokensScored.value += activeList.size();
            totalTokensScored.value += activeList.size();

            return moreTokens;
        }


        /**
        /// Keeps track of and reports all of the active word histories for the given active list
         *
        /// @param activeList the active list to track
         */
        private void monitorWords(ActiveList activeList)
        {
            //        WordTracker tracker1 = new WordTracker(currentFrameNumber);
            //
            //        for (Token t : activeList) {
            //            tracker1.add(t);
            //        }
            //        tracker1.dump();
            //        
            //        TokenTracker tracker2 = new TokenTracker();
            //
            //        for (Token t : activeList) {
            //            tracker2.add(t);
            //        }
            //        tracker2.dumpSummary();
            //        tracker2.dumpDetails();
            //        
            //        TokenTypeTracker tracker3 = new TokenTypeTracker();
            //
            //        for (Token t : activeList) {
            //            tracker3.add(t);
            //        }
            //        tracker3.dump();

            //        StateHistoryTracker tracker4 = new StateHistoryTracker(currentFrameNumber);

            //        for (Token t : activeList) {
            //            tracker4.add(t);
            //        }
            //        tracker4.dump();
        }


        /**
        /// Keeps track of and reports statistics about the number of active states
         *
        /// @param activeList the active list of states
         */
        private void monitorStates(ActiveList activeList)
        {

            tokenSum += activeList.size();
            tokenCount++;

            if ((tokenCount % 1000) == 0)
            {
                Trace.WriteLine("Average Tokens/State: " + (tokenSum / tokenCount));
            }
        }


        /** Removes unpromising branches from the active list */
        protected void pruneBranches()
        {
            pruneTimer.start();
            activeList = pruner.prune(activeList);
            pruneTimer.stop();
        }


        /**
        /// Gets the best token for this state
         *
        /// @param state the state of interest
        /// @return the best token
         */
        protected Token getBestToken(ISearchState state)
        {
            Object key = getStateKey(state);
            return bestTokenMap[key];
        }


        /**
        /// Sets the best token for a given state
         *
        /// @param token the best token
        /// @param state the state
         */
        protected void setBestToken(Token token, ISearchState state)
        {
            Object key = getStateKey(state);
            bestTokenMap.Add(key, token);
        }

        /**
        /// Returns the state key for the given state. This key is used
        /// to store bestToken into the bestToken map. All tokens with 
        /// the same key are basically shared. This method adds flexibility in
        /// search. 
        /// 
        /// For example this key will allow HMM states that have identical word 
        /// histories and are in the same HMM state to be treated equivalently. 
        /// When used  as the best token key, only the best scoring token with a 
        /// given word history survives per HMM. 
        /// <pre>
        ///   Boolean equal = hmmSearchState.getLexState().equals(
        ///          other.hmmSearchState.getLexState())
        ///          && hmmSearchState.getWordHistory().equals(
        ///          other.hmmSearchState.getWordHistory());                       
        /// </pre>
        /// 
        /// @param state
        ///            the state to get the key for
        /// @return the key for the given state
         */
        protected Object getStateKey(ISearchState state)
        {
            return state;
        }


        /** Checks that the given two states are in legitimate order.
        /// @param fromState
        /// @param toState*/
        private void checkStateOrder(ISearchState fromState, ISearchState toState)
        {
            if (fromState.getOrder() == numStateOrder - 1)
            {
                return;
            }

            if (fromState.getOrder() > toState.getOrder())
            {
                throw new Exception("IllegalState order: from "
                        + fromState.GetType().Name + ' '
                        + fromState.toPrettyString()
                        + " order: " + fromState.getOrder()
                        + " to "
                        + toState.GetType().Name + ' '
                        + toState.toPrettyString()
                        + " order: " + toState.getOrder());
            }
        }


        /**
        /// Collects the next set of emitting tokens from a token and accumulates them in the active or result lists
         *
        /// @param token the token to collect successors from be immediately expanded are placed. Null if we should always
        ///              expand all nodes.
         */
        protected void collectSuccessorTokens(Token token)
        {

            // tokenTracker.add(token);
            // tokenTypeTracker.add(token);

            // If this is a final state, add it to the final list

            if (token.isFinal())
            {
                resultList.Add(getResultListPredecessor(token));
                return;
            }

            // if this is a non-emitting token and we've already 
            // visited the same state during this frame, then we
            // are in a grammar loop, so we don't continue to expand.
            // This check only works properly if we have kept all of the
            // tokens (instead of skipping the non-word tokens).
            // Note that certain linguists will never generate grammar loops
            // (lextree linguist for example). For these cases, it is perfectly
            // fine to disable this check by setting keepAllTokens to false

            if (!token.isEmitting() && (keepAllTokens && isVisited(token)))
            {
                return;
            }

            ISearchState state = token.getSearchState();
            ISearchStateArc[] arcs = state.getSuccessors();
            Token predecessor = getResultListPredecessor(token);

            // For each successor
            // calculate the entry score for the token based upon the
            // predecessor token score and the transition probabilities
            // if the score is better than the best score encountered for
            // the SearchState and frame then create a new token, add
            // it to the lattice and the SearchState.
            // If the token is an emitting token add it to the list,
            // otherwise recursively collect the new tokens successors.

            foreach (ISearchStateArc arc in arcs)
            {
                ISearchState nextState = arc.getState();

                if (_checkStateOrder)
                {
                    checkStateOrder(state, nextState);
                }

                // We're actually multiplying the variables, but since
                // these come in log(), multiply gets converted to add
                float logEntryScore = token.getScore() + arc.getProbability();

                Token bestToken = getBestToken(nextState);

                //               


                if (bestToken == null)
                {
                    Token newBestToken = new Token(predecessor, nextState, logEntryScore, arc.getInsertionProbability(),
                            arc.getLanguageProbability(), currentFrameNumber);
                    tokensCreated.value++;
                    setBestToken(newBestToken, nextState);
                    activeListAdd(newBestToken);
                }
                else if (bestToken.getScore() < logEntryScore)
                {
                    // System.out.println("Updating " + bestToken + " with " +
                    // newBestToken);
                    Token oldPredecessor = bestToken.getPredecessor();
                    bestToken.update(predecessor, nextState, logEntryScore, arc.getInsertionProbability(),
                            arc.getLanguageProbability(), currentFrameNumber);
                    if (buildWordLattice && nextState is IWordSearchState)
                    {
                        loserManager.addAlternatePredecessor(bestToken, oldPredecessor);
                    }
                }
                else if (buildWordLattice && nextState is IWordSearchState)
                {
                    if (predecessor != null)
                    {
                        loserManager.addAlternatePredecessor(bestToken, predecessor);
                    }
                }
            }
        }


        /**
        /// Determines whether or not we've visited the state associated with this token since the previous frame.
         *
        /// @param t
        /// @return true if we've visited the search state since the last frame
         */
        private Boolean isVisited(Token t)
        {
            ISearchState curState = t.getSearchState();

            t = t.getPredecessor();

            while (t != null && !t.isEmitting())
            {
                if (curState.Equals(t.getSearchState()))
                {
                    Trace.WriteLine("CS " + curState + " match " + t.getSearchState());
                    return true;
                }
                t = t.getPredecessor();
            }
            return false;
        }


        protected void activeListAdd(Token token)
        {
            activeListManager.add(token);
        }


        //protected void activeListReplace(Token old, Token newToken)
        //{
        //    activeListManager.replace(old, newToken);
        //}


        /**
        /// Determine if the given token should be expanded
         *
        /// @param t the token to test
        /// @return <code>true</code> if the token should be expanded
         */
        protected Boolean allowExpansion(Token t)
        {
            return true; // currently disabled
        }


        /** Counts all the tokens in the active list (and displays them). This is an expensive operation. */
        private void showTokenCount()
        {
            HashSet<Token> tokenSet = new HashSet<Token>();

            foreach (Token token in activeList.getTokens())
            {
                while (token != null)
                {
                    tokenSet.Add(token);
                    //TODO: CHECK SEMANTICS
                    //token = token.getPredecessor();
                }
            }

            Trace.WriteLine("Token Lattice size: " + tokenSet.Count);

            tokenSet = new HashSet<Token>();

            foreach (Token token in resultList)
            {
                while (token != null)
                {
                    tokenSet.Add(token);
                    //token = token.getPredecessor();
                }
            }

            Trace.WriteLine("Result Lattice size: " + tokenSet.Count);
        }

        /**
        /// Returns the ActiveList.
         *
        /// @return the ActiveList
         */
        public ActiveList getActiveList()
        {
            return activeList;
        }


        /**
        /// Sets the ActiveList.
         *
        /// @param activeList the new ActiveList
         */
        public void setActiveList(ActiveList activeList)
        {
            this.activeList = activeList;
        }


        /**
        /// Returns the result list.
         *
        /// @return the result list
         */
        public List<Token> getResultList()
        {
            return resultList;
        }


        /**
        /// Sets the result list.
         *
        /// @param resultList the new result list
         */
        public void setResultList(List<Token> resultList)
        {
            this.resultList = resultList;
        }


        /**
        /// Returns the current frame number.
         *
        /// @return the current frame number
         */
        public int getCurrentFrameNumber()
        {
            return currentFrameNumber;
        }


        /**
        /// Returns the Timer for growing.
         *
        /// @return the Timer for growing
         */
        public Timer getGrowTimer()
        {
            return growTimer;
        }


        /**
        /// Returns the tokensCreated StatisticsVariable.
         *
        /// @return the tokensCreated StatisticsVariable.
         */
        public StatisticsVariable getTokensCreated()
        {
            return tokensCreated;
        }

    }
}
