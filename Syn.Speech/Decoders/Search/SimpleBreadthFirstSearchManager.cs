using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Syn.Logging;
using Syn.Speech.Decoders.Pruner;
using Syn.Speech.Decoders.Scorer;
using Syn.Speech.Helper;
using Syn.Speech.Linguist;
using Syn.Speech.Results;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
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
        [S4Component(Type = typeof(Linguist.Linguist))]
        public static string PropLinguist = "linguist";

        /// <summary>
        /// The property that defines the name of the linguist to be used by this search manager.
        /// </summary>
        [S4Component(Type = typeof(IPruner))]
        public static string PropPruner = "pruner";

        /// <summary>
        /// The property that defines the name of the scorer to be used by this search manager.
        /// </summary>
        [S4Component(Type = typeof(IAcousticScorer))]
        public static string PropScorer = "scorer";

        /// <summary>
        /// The property that defines the name of the active list factory to be used by this search manager. 
        /// </summary>
        [S4Component(Type = typeof(ActiveListFactory))]
        public static string PropActiveListFactory = "activeListFactory";

        /// <summary>
        ///         
        /// The property that when set to <code>true</code> will cause the recognizer to count up all the tokens in the
        /// active list after every frame.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropShowTokenCount = "showTokenCount";

        /// <summary>
        /// The property that sets the minimum score relative to the maximum score in the word list for pruning. Words with a
        /// score less than relativeBeamWidth/// maximumScore will be pruned from the list
        /// </summary>
        [S4Double(DefaultValue = 0.0)]
        public static string PropRelativeWordBeamWidth = "relativeWordBeamWidth";

        /// <summary>
        /// The property that controls whether or not relative beam pruning will be performed on the entry into a
        /// state.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropWantEntryPruning = "wantEntryPruning";

        /// <summary>
        /// The property that controls the number of frames processed for every time the decode growth step is skipped.
        /// Setting this property to zero disables grow skipping. Setting this number to a small integer will increase the
        /// speed of the decoder but will also decrease its accuracy. The higher the number, the less often the grow code is
        /// skipped.
        /// </summary>
        [S4Integer(DefaultValue = 0)]
        public static string PropGrowSkipInterval = "growSkipInterval";


        protected Linguist.Linguist Linguist = null; // Provides grammar/language info
        private IPruner _pruner; // used to prune the active list
        private IAcousticScorer _scorer; // used to score the active list
        protected ActiveList ActiveList; // the list of active tokens
        protected List<Token> ResultList; // the current set of results
        protected LogMath LogMath;

        private string _name;

        // ------------------------------------
        // monitoring data
        // ------------------------------------

        private Timer _scoreTimer; // TODO move these timers out
        private Timer _pruneTimer;
        private StatisticsVariable _totalTokensScored;
        private StatisticsVariable _tokensPerSecond;
        private StatisticsVariable _curTokensScored;
        private StatisticsVariable _viterbiPruned;
        private StatisticsVariable _beamPruned;

        // ------------------------------------
        // Working data
        // ------------------------------------

        protected Boolean _showTokenCount = false;
        private Boolean _wantEntryPruning;
        protected HashMap<ISearchState, Token> BestTokenMap;
        private float _logRelativeWordBeamWidth;
        private int _totalHmms;
        private double _startTime;
        private float _threshold;
        private float _wordThreshold;
        private int _growSkipInterval;
        protected ActiveListFactory ActiveListFactory;
        protected Boolean StreamEnd;

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
                                               int growSkipInterval, Boolean wantEntryPruning, int totalHmms)
        {
            _name = GetType().Name;
            LogMath = LogMath.GetLogMath();
            Linguist = linguist;
            _pruner = pruner;
            _scorer = scorer;
            ActiveListFactory = activeListFactory;
            _showTokenCount = showTokenCount;
            _growSkipInterval = growSkipInterval;
            _wantEntryPruning = wantEntryPruning;
            _totalHmms = totalHmms;
            _logRelativeWordBeamWidth = LogMath.LinearToLog(relativeWordBeamWidth);
            KeepAllTokens = true;
        }

        public override void NewProperties(PropertySheet ps)
        {
            LogMath = LogMath.GetLogMath();
            _name = ps.InstanceName;

            Linguist = (Linguist.Linguist)ps.GetComponent(PropLinguist);
            _pruner = (IPruner)ps.GetComponent(PropPruner);
            _scorer = (IAcousticScorer)ps.GetComponent(PropScorer);
            ActiveListFactory = (ActiveListFactory)ps.GetComponent(PropActiveListFactory);
            _showTokenCount = ps.GetBoolean(PropShowTokenCount);

            double relativeWordBeamWidth = ps.GetDouble(PropRelativeWordBeamWidth);
            _growSkipInterval = ps.GetInt(PropGrowSkipInterval);
            _wantEntryPruning = ps.GetBoolean(PropWantEntryPruning);
            _logRelativeWordBeamWidth = LogMath.LinearToLog(relativeWordBeamWidth);

            KeepAllTokens = true;
        }


        /// <summary>
        /// Called at the start of recognition. Gets the search manager ready to recognize.
        /// </summary>
        public override void StartRecognition()
        {
            this.LogInfo("starting recognition");

            Linguist.StartRecognition();
            _pruner.StartRecognition();
            _scorer.StartRecognition();
            LocalStart();
            if (_startTime == 0.0)
            {
                _startTime = Java.CurrentTimeMillis();
            }
        }


        /**
        /// Performs the recognition for the given number of frames.
         *
        /// @param nFrames the number of frames to recognize
        /// @return the current result or null if there is no Result (due to the lack of frames to recognize)
         */
        public override Result Recognize(int nFrames)
        {
            bool done = false;
            Result result = null;
            StreamEnd = false;

            for (int i = 0; i < nFrames && !done; i++)
            {
                done = Recognize();
            }

            // generate a new temporary result if the current token is based on a final search state
            // remark: the first check for not null is necessary in cases that the search space does not contain scoreable tokens.
            if (ActiveList.GetBestToken() != null)
            {
                // to make the current result as correct as possible we undo the last search graph expansion here
                ActiveList fixedList = UndoLastGrowStep();

                // Now create the result using the fixed active-list.
                if (!StreamEnd)
                {
                    result = new Result(fixedList, ResultList, CurrentFrameNumber, done, false);
                }
            }

            if (_showTokenCount)
            {
                ShowTokenCount();
            }

            return result;
        }


        /**
        /// Because the growBranches() is called although no data is left after the last speech frame, the ordering of the
        /// active-list might depend on the transition probabilities and (penalty-scores) only. Therefore we need to undo the last
        /// grow-step up to final states or the last emitting state in order to fix the list.
        /// @return newly created list
         */
        protected ActiveList UndoLastGrowStep()
        {
            var fixedList = ActiveList.NewInstance();

            foreach (var token in ActiveList)
            {
                var curToken = token.Predecessor;

                // remove the final states that are not the real final ones because they're just hide prior final tokens:
                while (curToken.Predecessor != null && (
                        (curToken.IsFinal && curToken.Predecessor != null && !curToken.Predecessor.IsFinal)
                                || (curToken.IsEmitting && curToken.Data == null) // the so long not scored tokens
                                || (!curToken.IsFinal && !curToken.IsEmitting)))
                {
                    curToken = curToken.Predecessor;
                }

                fixedList.Add(curToken);
            }

            return fixedList;
        }


        /// <summary>
        /// Terminates a recognition
        /// </summary>
        public override void StopRecognition()
        {
            LocalStop();
            _scorer.StopRecognition();
            _pruner.StopRecognition();
            Linguist.StopRecognition();

            this.LogInfo("recognition stopped");
        }


        /**
        /// Performs recognition for one frame. Returns true if recognition has been completed.
         *
        /// @return <code>true</code> if recognition is completed.
         */

        public bool Recognize()
        {

            bool more = ScoreTokens(); // score emitting tokens
            if (more)
            {
                PruneBranches(); // eliminate poor branches
                CurrentFrameNumber++;
                this.LogDebug("Current Frame: {0}", CurrentFrameNumber);
                if (_growSkipInterval == 0 || (CurrentFrameNumber % _growSkipInterval) != 0)
                {
                    GrowBranches(); // extend remaining branches
                }
            }
            return !more;
        }


        /// <summary>
        /// Gets the initial grammar node from the linguist and creates a GrammarNodeToken.
        /// </summary>
        protected void LocalStart()
        {
            CurrentFrameNumber = 0;
            _curTokensScored.Value = 0;
            ActiveList newActiveList = ActiveListFactory.NewInstance();
            ISearchState state = Linguist.SearchGraph.InitialState;
            newActiveList.Add(new Token(state, CurrentFrameNumber));
            ActiveList = newActiveList;

            GrowBranches();
        }


        /** Local cleanup for this search manager */
        protected void LocalStop()
        {
        }


        /**
        /// Goes through the active list of tokens and expands each token, finding the set of successor tokens until all the
        /// successor tokens are emitting tokens.
         */
        protected void GrowBranches()
        {
            int mapSize = ActiveList.Size * 10;
            if (mapSize == 0)
            {
                mapSize = 1;
            }
            GrowTimer.Start();
            BestTokenMap = new HashMap<ISearchState, Token>(mapSize);
            ActiveList oldActiveList = ActiveList;
            ResultList = new List<Token>();
            ActiveList = ActiveListFactory.NewInstance();
            _threshold = oldActiveList.GetBeamThreshold();
            _wordThreshold = oldActiveList.GetBestScore() + _logRelativeWordBeamWidth;

            foreach (Token token in oldActiveList)
            {
                CollectSuccessorTokens(token);
            }
            GrowTimer.Stop();

        }


        /// <summary>
        /// Calculate the acoustic scores for the active list. The active list should contain only emitting tokens.
        /// </summary>
        /// <returns><code>true</code> if there are more frames to score, otherwise, false</returns>
        protected bool ScoreTokens()
        {
            var hasMoreFrames = false;

            _scoreTimer.Start();
            this.LogDebug("Calculating score for an activelist of Size: {0}", ActiveList.Count());
            var data = _scorer.CalculateScores(ActiveList.GetTokens());
            this.LogDebug("Score Data: {0}", data);
            _scoreTimer.Stop();

            Token bestToken = null;
            if (data is Token)
            {
                bestToken = (Token)data;
            }
            else if (data == null)
            {
                StreamEnd = true;
            }

            if (bestToken != null)
            {
                hasMoreFrames = true;
                ActiveList.SetBestToken(bestToken);
            }

            // update statistics
            _curTokensScored.Value += ActiveList.Size;
            _totalTokensScored.Value += ActiveList.Size;
            _tokensPerSecond.Value = _totalTokensScored.Value / GetTotalTime();

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
        private double GetTotalTime()
        {
            return (Java.CurrentTimeMillis() - _startTime) / 1000.0;
        }


        /// <summary>
        /// Removes unpromising branches from the active list.
        /// </summary>
        protected void PruneBranches()
        {
            int startSize = ActiveList.Size;
            _pruneTimer.Start();
            ActiveList = _pruner.Prune(ActiveList);
            _beamPruned.Value += startSize - ActiveList.Size;
            _pruneTimer.Stop();
        }


        /**
        /// Gets the best token for this state
         *
        /// @param state the state of interest
        /// @return the best token
         */
        protected Token GetBestToken(ISearchState state)
        {
            Token best = null;
            if (BestTokenMap.ContainsKey(state))
            {
                best = BestTokenMap[state];
                //this.LogInfo("BT " + best + " for state " + state);
            }

            return best;
        }


        protected Token SetBestToken(Token token, ISearchState state)
        {
            return BestTokenMap.Put(state, token);
        }


        public ActiveList GetActiveList()
        {
            return ActiveList;
        }


        /**
        /// Collects the next set of emitting tokens from a token and accumulates them in the active or result lists
         *
        /// @param token the token to collect successors from
         */
        protected void CollectSuccessorTokens(Token token)
        {
            ISearchState state = token.SearchState;
            // If this is a final state, add it to the final list
            if (token.IsFinal)
            {
                ResultList.Add(token);
            }
            if (token.Score < _threshold)
            {
                return;
            }
            if (state is IWordSearchState
                    && token.Score < _wordThreshold)
            {
                return;
            }
            ISearchStateArc[] arcs = state.GetSuccessors();
            //this.LogDebug("Arcs Count: {0}", arcs.Length);
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
                ISearchState nextState = arc.State;
                // this.LogDebug("Next State: {0}",nextState);
                // We're actually multiplying the variables, but since
                // these come in log(), multiply gets converted to add
                float logEntryScore = token.Score + arc.GetProbability();
                //this.LogDebug("LogEntryScore: {0}", logEntryScore);
                if (_wantEntryPruning)
                { // false by default
                    if (logEntryScore < _threshold)
                    {
                        continue;
                    }
                    if (nextState is IWordSearchState
                            && logEntryScore < _wordThreshold)
                    {
                        continue;
                    }
                }
                Token predecessor = GetResultListPredecessor(token);
                // this.LogDebug("Predecessor: {0}", predecessor);
                // if not emitting, check to see if we've already visited
                // this state during this frame. Expand the token only if we
                // haven't visited it already. This prevents the search
                // from getting stuck in a loop of states with no
                // intervening emitting nodes. This can happen with nasty
                // jsgf grammars such as ((foo*)*)*
                if (!nextState.IsEmitting)
                {
                    Token newToken = new Token(predecessor, nextState, logEntryScore,
                            arc.InsertionProbability,
                            arc.LanguageProbability,
                            CurrentFrameNumber);
                    TokensCreated.Value++;
                    if (!IsVisited(newToken))
                    {
                        CollectSuccessorTokens(newToken);
                    }
                    continue;
                }

                Token bestToken = GetBestToken(nextState);
                // this.LogDebug("BestToken: {0}", bestToken);
                if (bestToken == null)
                {
                    Token newToken = new Token(predecessor, nextState, logEntryScore,
                            arc.InsertionProbability,
                            arc.LanguageProbability,
                            CurrentFrameNumber);
                    TokensCreated.Value++;
                    SetBestToken(newToken, nextState);
                    //this.LogDebug("Adding Token: {0}", newToken);
                    ActiveList.Add(newToken);
                }
                else
                {
                    if (bestToken.Score <= logEntryScore)
                    {
                        bestToken.Update(predecessor, nextState, logEntryScore,
                                arc.InsertionProbability,
                                arc.LanguageProbability,
                                CurrentFrameNumber);
                        _viterbiPruned.Value++;
                    }
                    else
                    {
                        _viterbiPruned.Value++;
                    }
                }
            }
        }


        /**
        /// Determines whether or not we've visited the state associated with this token since the previous frame.
         *
        /// @param t the token to check
        /// @return true if we've visited the search state since the last frame
         */
        private static Boolean IsVisited(Token t)
        {
            ISearchState curState = t.SearchState;

            t = t.Predecessor;

            while (t != null && !t.IsEmitting)
            {
                if (curState.Equals(t.SearchState))
                {
                    return true;
                }
                t = t.Predecessor;
            }
            return false;
        }


        /** Counts all the tokens in the active list (and displays them). This is an expensive operation. */
        protected void ShowTokenCount()
        {
            var tokenSet = new HashSet<Token>();
            foreach (Token tk in ActiveList)
            {
                Token token = tk;
                while (token != null)
                {
                    tokenSet.Add(token);
                    token = token.Predecessor;
                }
            }
            this.LogInfo("Token Lattice size: " + tokenSet.Count);
            tokenSet = new HashSet<Token>();
            foreach (Token tk in ResultList)
            {
                Token token = tk;
                while (token != null)
                {
                    tokenSet.Add(token);
                    token = token.Predecessor;
                }
            }
            this.LogInfo("Result Lattice size: " + tokenSet.Count);
        }


        /**
        /// Returns the best token map.
         *
        /// @return the best token map
         */
        protected HashMap<ISearchState , Token> GetBestTokenMap()
        {
            return BestTokenMap;
        }


        /**
        /// Sets the best token Map.
         *
        /// @param bestTokenMap the new best token Map
         */
        protected void SetBestTokenMap(HashMap<ISearchState, Token> bestTokenMap)
        {
            BestTokenMap = bestTokenMap;
        }


        /**
        /// Returns the result list.
         *
        /// @return the result list
         */
        public List<Token> GetResultList()
        {
            return ResultList;
        }


        /**
        /// Returns the current frame number.
         *
        /// @return the current frame number
         */

        public int CurrentFrameNumber{ get; set; }


        /**
        /// Returns the Timer for growing.
         *
        /// @return the Timer for growing
         */

        public Timer GrowTimer { get; protected set; }


        /**
        /// Returns the tokensCreated StatisticsVariable.
         *
        /// @return the tokensCreated StatisticsVariable.
         */

        public StatisticsVariable TokensCreated { get; private set; }


        /// <summary>
        /// @see Search.SearchManager#allocate()
        /// </summary>
        public override void Allocate()
        {
            _totalTokensScored = StatisticsVariable.GetStatisticsVariable("totalTokensScored");
            _tokensPerSecond = StatisticsVariable.GetStatisticsVariable("tokensScoredPerSecond");
            _curTokensScored = StatisticsVariable.GetStatisticsVariable("curTokensScored");
            TokensCreated = StatisticsVariable.GetStatisticsVariable("tokensCreated");
            _viterbiPruned = StatisticsVariable.GetStatisticsVariable("viterbiPruned");
            _beamPruned = StatisticsVariable.GetStatisticsVariable("beamPruned");

            try
            {
                Linguist.Allocate();
                _pruner.Allocate();
                _scorer.Allocate();
            }
            catch (IOException e)
            {
                throw new SystemException("Allocation of search manager resources failed", e);
            }

            _scoreTimer = TimerPool.GetTimer(this, "Score");
            _pruneTimer = TimerPool.GetTimer(this, "Prune");
            GrowTimer = TimerPool.GetTimer(this, "Grow");
        }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.decoder.search.SearchManager#deallocate()
        */
        public override void Deallocate()
        {
            try
            {
                _scorer.Deallocate();
                _pruner.Deallocate();
                Linguist.Deallocate();
            }
            catch (IOException e)
            {
                throw new SystemException("Deallocation of search manager resources failed", e);
            }
        }


        public override string ToString()
        {
            return _name;
        }
    }
}
