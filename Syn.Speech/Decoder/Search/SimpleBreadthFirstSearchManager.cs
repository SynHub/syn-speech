using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Syn.Speech.Common.FrontEnd;
using Syn.Speech.Decoder.Pruner;
using Syn.Speech.Decoder.Scorer;
using Syn.Speech.Helper;
using Syn.Speech.Linguist;
using Syn.Speech.Results;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// Provides the breadth first search. To perform recognition an application should call initialize before recognition
    /// begins, and repeatedly call <code> recognize </code> until Result.isFinal() returns true. Once a final result has
    /// been obtained, <code> terminate </code> should be called.
    /// 
    /// All scores and probabilities are maintained in the log math log domain.
    /// 
    /// For information about breadth first search please refer to "Spoken Language Processing", X. Huang, PTR
    /// 
    /// </summary>
    public class SimpleBreadthFirstSearchManager : TokenSearchManager
    {
            
        /// <summary>
        /// The property that defines the name of the linguist to be used by this search manager. 
        /// </summary>
        [S4Component(type=typeof(Linguist.Linguist))]
        public static String PROP_LINGUIST = "linguist";

        /// <summary>
        /// The property that defines the name of the linguist to be used by this search manager.
        /// </summary>
        [S4Component(type=typeof(IPruner))]
        public static String PROP_PRUNER = "pruner";

        /// <summary>
        /// The property that defines the name of the scorer to be used by this search manager.
        /// </summary>
        [S4Component(type=typeof(IAcousticScorer))]
        public static String PROP_SCORER = "scorer";

        /// <summary>
        /// The property that defines the name of the active list factory to be used by this search manager. 
        /// </summary>
        [S4Component(type=typeof(ActiveListFactory))]
        public static String PROP_ACTIVE_LIST_FACTORY = "activeListFactory";

        /// <summary>
        ///         
        /// The property that when set to <code>true</code> will cause the recognizer to count up all the tokens in the
        /// active list after every frame.
        /// </summary>
        [S4Boolean(defaultValue = false)]
        public static String PROP_SHOW_TOKEN_COUNT = "showTokenCount";

        /// <summary>
        /// The property that sets the minimum score relative to the maximum score in the word list for pruning. Words with a
        /// score less than relativeBeamWidth/// maximumScore will be pruned from the list
        /// </summary>
        [S4Double(defaultValue = 0.0)]
        public static String PROP_RELATIVE_WORD_BEAM_WIDTH = "relativeWordBeamWidth";

        /// <summary>
        /// The property that controls whether or not relative beam pruning will be performed on the entry into a
        /// state.
        /// </summary>
        [S4Boolean(defaultValue = false)]
        public static String PROP_WANT_ENTRY_PRUNING = "wantEntryPruning";

        /// <summary>
        /// The property that controls the number of frames processed for every time the decode growth step is skipped.
        /// Setting this property to zero disables grow skipping. Setting this number to a small integer will increase the
        /// speed of the decoder but will also decrease its accuracy. The higher the number, the less often the grow code is
        /// skipped.
        /// </summary>
        [S4Integer(defaultValue = 0)]
        public static String PROP_GROW_SKIP_INTERVAL = "growSkipInterval";


        protected Linguist.Linguist linguist=null; // Provides grammar/language info
        private IPruner pruner=null; // used to prune the active list
        private IAcousticScorer scorer=null; // used to score the active list
        protected int currentFrameNumber; // the current frame number
        protected ActiveList activeList; // the list of active tokens
        protected List<Token> resultList; // the current set of results
        protected LogMath logMath;

        private String name;

        // ------------------------------------
        // monitoring data
        // ------------------------------------

        private Timer scoreTimer; // TODO move these timers out
        private Timer pruneTimer;
        protected Timer growTimer;
        private StatisticsVariable totalTokensScored;
        private StatisticsVariable tokensPerSecond;
        private StatisticsVariable curTokensScored;
        private StatisticsVariable tokensCreated;
        private StatisticsVariable viterbiPruned;
        private StatisticsVariable beamPruned;

        // ------------------------------------
        // Working data
        // ------------------------------------

        protected Boolean _showTokenCount=false;
        private Boolean wantEntryPruning;
        protected Dictionary<ISearchState, Token> bestTokenMap;
        private float logRelativeWordBeamWidth;
        private int totalHmms;
        private double startTime;
        private float threshold;
        private float wordThreshold;
        private int growSkipInterval;
        protected ActiveListFactory activeListFactory;
        protected Boolean streamEnd;

        public SimpleBreadthFirstSearchManager() 
        {
        
        }

        /**
        /// 
        /// @param linguist
        /// @param pruner
        /// @param scorer
        /// @param activeListFactory
        /// @param showTokenCount
        /// @param relativeWordBeamWidth
        /// @param growSkipInterval
        /// @param wantEntryPruning
         */
        public SimpleBreadthFirstSearchManager(Linguist.Linguist linguist, IPruner pruner,
                                               IAcousticScorer scorer, ActiveListFactory activeListFactory,
                                               Boolean showTokenCount, double relativeWordBeamWidth,
                                               int growSkipInterval, Boolean wantEntryPruning) 
        {
            this.name = GetType().Name;
            this.logMath = LogMath.getLogMath();
            this.linguist = linguist;
            this.pruner = pruner;
            this.scorer = scorer;
            this.activeListFactory = activeListFactory;
            this._showTokenCount = showTokenCount;
            this.growSkipInterval = growSkipInterval;
            this.wantEntryPruning = wantEntryPruning;
            this.logRelativeWordBeamWidth = logMath.linearToLog(relativeWordBeamWidth);
            this.keepAllTokens = true;
        }

        override public void newProperties(PropertySheet ps)
        {        
            logMath = LogMath.getLogMath();
            name = ps.InstanceName;

            linguist = (Linguist.Linguist)ps.getComponent(PROP_LINGUIST);
            pruner = (IPruner) ps.getComponent(PROP_PRUNER);
            scorer = (IAcousticScorer) ps.getComponent(PROP_SCORER);
            activeListFactory = (ActiveListFactory) ps.getComponent(PROP_ACTIVE_LIST_FACTORY);
            _showTokenCount = ps.getBoolean(PROP_SHOW_TOKEN_COUNT);

            double relativeWordBeamWidth = ps.getDouble(PROP_RELATIVE_WORD_BEAM_WIDTH);
            growSkipInterval = ps.getInt(PROP_GROW_SKIP_INTERVAL);
            wantEntryPruning = ps.getBoolean(PROP_WANT_ENTRY_PRUNING);
            logRelativeWordBeamWidth = logMath.linearToLog(relativeWordBeamWidth);
        
            this.keepAllTokens = true;      
        }


        /** Called at the start of recognition. Gets the search manager ready to recognize */
        override public void startRecognition() 
        {
            Trace.WriteLine("starting recognition");

            linguist.startRecognition();
            pruner.startRecognition();
            scorer.startRecognition();
            localStart();
            if (startTime == 0.0) {
                startTime = Extensions.currentTimeMillis();
            }
        }


        /**
        /// Performs the recognition for the given number of frames.
         *
        /// @param nFrames the number of frames to recognize
        /// @return the current result or null if there is no Result (due to the lack of frames to recognize)
         */
        override public Result recognize(int nFrames) 
        {
            bool done = false;
            Result result = null;
            streamEnd = false;
 
            for (int i = 0; i < nFrames && !done; i++) 
            {
                done = recognize();
            }

            // generate a new temporary result if the current token is based on a final search state
            // remark: the first check for not null is necessary in cases that the search space does not contain scoreable tokens.
            if (activeList.getBestToken() != null) 
            {
                // to make the current result as correct as possible we undo the last search graph expansion here
                ActiveList fixedList = undoLastGrowStep();
            	
                // Now create the result using the fixed active-list.
                if (!streamEnd)
                {
                    result = new Results.Result(fixedList,resultList, currentFrameNumber, done);
                }
            }

            if (_showTokenCount) {
                showTokenCount();
            }

            return result;
        }


        /**
        /// Because the growBranches() is called although no data is left after the last speech frame, the ordering of the
        /// active-list might depend on the transition probabilities and (penalty-scores) only. Therefore we need to undo the last
        /// grow-step up to final states or the last emitting state in order to fix the list.
        /// @return newly created list
         */
        protected ActiveList undoLastGrowStep() 
        {
            var fixedList = activeList.newInstance();

            foreach (var token in activeList.getTokens()) 
            {
                var curToken = token.getPredecessor();

                // remove the final states that are not the real final ones because they're just hide prior final tokens:
                while (curToken.getPredecessor() != null && (
                        (curToken.isFinal() && curToken.getPredecessor() != null && !curToken.getPredecessor().isFinal())
                                || (curToken.isEmitting() && curToken.getData() == null) // the so long not scored tokens
                                || (!curToken.isFinal() && !curToken.isEmitting()))) {
                    curToken = curToken.getPredecessor();
                }

                fixedList.add(curToken);
            }

            return fixedList;
        }


        /// <summary>
        /// Terminates a recognition
        /// </summary>
        override public void stopRecognition() 
        {
            localStop();
            scorer.stopRecognition();
            pruner.stopRecognition();
            linguist.stopRecognition();

            Trace.WriteLine("recognition stopped");
        }


        /**
        /// Performs recognition for one frame. Returns true if recognition has been completed.
         *
        /// @return <code>true</code> if recognition is completed.
         */
        protected bool recognize() 
        {
            bool more = scoreTokens(); // score emitting tokens
            if (more) 
            {
                pruneBranches(); // eliminate poor branches
                currentFrameNumber++;
                if (growSkipInterval == 0
                        || (currentFrameNumber % growSkipInterval) != 0) {
                    growBranches(); // extend remaining branches
                }
            }
            return !more;
        }


        /// <summary>
        /// Gets the initial grammar node from the linguist and creates a GrammarNodeToken.
        /// </summary>
        protected void localStart() 
        {
            currentFrameNumber = 0;
            curTokensScored.value = 0;
            ActiveList newActiveList = activeListFactory.newInstance();
            ISearchState state = linguist.getSearchGraph().getInitialState();
            newActiveList.add(new Token(state, currentFrameNumber));
            activeList = newActiveList;

            growBranches();
        }


        /** Local cleanup for this search manager */
        protected void localStop() 
        {
        }


        /**
        /// Goes through the active list of tokens and expands each token, finding the set of successor tokens until all the
        /// successor tokens are emitting tokens.
         */
        protected void growBranches() 
        {
            int mapSize = activeList.size()*10;
            if (mapSize == 0) {
                mapSize = 1;
            }
            growTimer.start();
            bestTokenMap = new Dictionary<ISearchState, Token>(mapSize);
            ActiveList oldActiveList = activeList;
            resultList = new List<Token>();
            activeList = activeListFactory.newInstance();
            threshold = oldActiveList.getBeamThreshold();
            wordThreshold = oldActiveList.getBestScore() + logRelativeWordBeamWidth;

            foreach (Token token in oldActiveList.getTokens()) 
            {
                collectSuccessorTokens(token);
            }
            growTimer.stop();
            
            #if Debug
                int hmms = activeList.size();
                totalHmms += hmms;
                Trace.WriteLine("Frame: " + currentFrameNumber + " Hmms: "
                        + hmms + "  total " + totalHmms);
        	#endif
        }


        /// <summary>
        /// Calculate the acoustic scores for the active list. The active list should contain only emitting tokens.
        /// </summary>
        /// <returns><code>true</code> if there are more frames to score, otherwise, false</returns>
        protected bool scoreTokens() 
        {
            bool hasMoreFrames = false;

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
        
            if (bestToken != null) {
                hasMoreFrames = true;
                activeList.setBestToken(bestToken);
            }

            // update statistics
            curTokensScored.value += activeList.size();
            totalTokensScored.value += activeList.size();
            tokensPerSecond.value = totalTokensScored.value / getTotalTime();

    //        if (logger.isLoggable(Level.FINE)) {
    //            logger.fine(currentFrameNumber + " " + activeList.size()
    //                    + " " + curTokensScored.value + " "
    //                    + (int) tokensPerSecond.value);
    //        }

            return hasMoreFrames;
        }


        /**
        /// Returns the total time since we start4ed
         *
        /// @return the total time (in seconds)
         */
        private double getTotalTime() 
        {
            return (Extensions.currentTimeMillis() - startTime) / 1000.0;
        }


        /** Removes unpromising branches from the active list */
        protected void pruneBranches() 
        {
            int startSize = activeList.size();
            pruneTimer.start();
            activeList = pruner.prune(activeList);
            beamPruned.value += startSize - activeList.size();
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
            Token best = null;
            if (bestTokenMap.ContainsKey(state))
            {
                best = bestTokenMap[state];
                Trace.WriteLine("BT " + best + " for state " + state);
            }

            return best;
        }


        protected Token setBestToken(Token token, ISearchState state)
        {
             bestTokenMap.Add(state, token);
             return token;
        }


        public ActiveList getActiveList() 
        {
            return activeList;
        }


        /**
        /// Collects the next set of emitting tokens from a token and accumulates them in the active or result lists
         *
        /// @param token the token to collect successors from
         */
        protected void collectSuccessorTokens(Token token) 
        {
            ISearchState state = token.getSearchState();
            // If this is a final state, add it to the final list
            if (token.isFinal()) {
                resultList.Add(token);
            }
            if (token.getScore() < threshold) 
            {
                return;
            }
            if (state is IWordSearchState
                    && token.getScore() < wordThreshold) 
            {
                return;
            }
            ISearchStateArc[] arcs = state.getSuccessors();
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
                // We're actually multiplying the variables, but since
                // these come in log(), multiply gets converted to add
                float logEntryScore = token.getScore() + arc.getProbability();
                if (wantEntryPruning) { // false by default
                    if (logEntryScore < threshold) {
                        continue;
                    }
                    if (nextState is IWordSearchState
                            && logEntryScore < wordThreshold) {
                        continue;
                    }
                }
                Token predecessor = getResultListPredecessor(token);
                // if not emitting, check to see if we've already visited
                // this state during this frame. Expand the token only if we
                // haven't visited it already. This prevents the search
                // from getting stuck in a loop of states with no
                // intervening emitting nodes. This can happen with nasty
                // jsgf grammars such as ((foo*)*)*
                if (!nextState.isEmitting())
                {
                    Token newToken = new Token(predecessor, nextState, logEntryScore,
                            arc.getInsertionProbability(),
                            arc.getLanguageProbability(),
                            currentFrameNumber);
                    tokensCreated.value++;
                    if (!isVisited(newToken))
                    {
                        collectSuccessorTokens(newToken);
                    }
                    continue;
                }

                Token bestToken = getBestToken(nextState);
                if (bestToken == null)
                {
                    Token newToken = new Token(predecessor, nextState, logEntryScore,
                            arc.getInsertionProbability(),
                            arc.getLanguageProbability(),
                            currentFrameNumber);
                    tokensCreated.value++;
                    setBestToken(newToken, nextState);
                    activeList.add(newToken);
                }
                else
                {
                    if (bestToken.getScore() <= logEntryScore)
                    {
                        bestToken.update(predecessor as Token, nextState, logEntryScore,
                                arc.getInsertionProbability(),
                                arc.getLanguageProbability(),
                                currentFrameNumber);
                        viterbiPruned.value++;
                    }
                    else
                    {
                        viterbiPruned.value++;
                    }
                }


                //Token bestToken = getBestToken(nextState);
                //Boolean firstToken = bestToken == null;
                //if (firstToken || bestToken.getScore() <= logEntryScore) {
                //    Token newToken = new Token(predecessor, nextState, logEntryScore,
                //            arc.getInsertionProbability(),
                //            arc.getLanguageProbability(), 
                //            currentFrameNumber);
                //    tokensCreated.value++;
                //    setBestToken(newToken, nextState);
                //    if (!newToken.isEmitting()) {

                //        if (!isVisited(newToken)) {
                //            collectSuccessorTokens(newToken);
                //        }
                //    } else {
                //        if (firstToken) {
                //            activeList.add(newToken);
                //        } else {
                //            activeList.replace(bestToken, newToken);
                //            viterbiPruned.value++;
                //        }
                //    }
                //} else {
                //    viterbiPruned.value++;
                //}
            }
        }


        /**
        /// Determines whether or not we've visited the state associated with this token since the previous frame.
         *
        /// @param t the token to check
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
                    return true;
                }
                t = t.getPredecessor();
            }
            return false;
        }


        /** Counts all the tokens in the active list (and displays them). This is an expensive operation. */
        protected void showTokenCount() 
        {

                List<Token> tokenSet = new List<Token>();
                foreach (Token tk in activeList.getTokens()) 
                {
                    Token token = tk;
                    while (token != null) 
                    {
                        tokenSet.Add(token);
                        token = token.getPredecessor();
                    }
                }
                
                Trace.WriteLine("Token Lattice size: " + tokenSet.Count.ToString());
                tokenSet = new List<Token>();
                foreach (Token tk in resultList) 
                {
                    Token token = tk;
                    while (token != null) 
                    {
                        tokenSet.Add(token);
                        token = token.getPredecessor();
                    }
                }
                Trace.WriteLine("Result Lattice size: " + tokenSet.Count.ToString());

        }


        /**
        /// Returns the best token map.
         *
        /// @return the best token map
         */
        protected Dictionary<ISearchState, Token> getBestTokenMap() 
        {
            return bestTokenMap;
        }


        /**
        /// Sets the best token Map.
         *
        /// @param bestTokenMap the new best token Map
         */
        protected void setBestTokenMap(Dictionary<ISearchState, Token> bestTokenMap) 
        {
            this.bestTokenMap = bestTokenMap;
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


        /// <summary>
        /// @see Search.SearchManager#allocate()
        /// </summary>
        override public void allocate() 
        {
            totalTokensScored = StatisticsVariable.getStatisticsVariable("totalTokensScored");
            tokensPerSecond = StatisticsVariable.getStatisticsVariable("tokensScoredPerSecond");
            curTokensScored = StatisticsVariable.getStatisticsVariable("curTokensScored");
            tokensCreated = StatisticsVariable.getStatisticsVariable("tokensCreated");
            viterbiPruned = StatisticsVariable.getStatisticsVariable("viterbiPruned");
            beamPruned = StatisticsVariable.getStatisticsVariable("beamPruned");

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

            scoreTimer = TimerPool.getTimer(this, "Score");
            pruneTimer = TimerPool.getTimer(this, "Prune");
            growTimer = TimerPool.getTimer(this, "Grow");
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


        public override String ToString() 
        {
            return name;
        }
    }
}
