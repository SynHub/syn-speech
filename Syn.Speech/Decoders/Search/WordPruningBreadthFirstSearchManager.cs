using System;
using System.Collections.Generic;
using System.IO;
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
    /// been obtained, <code> stopRecognition </code> should be called.
    /// All scores and probabilities are maintained in the log math log domain.
    /// </summary>
    public class WordPruningBreadthFirstSearchManager : TokenSearchManager
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
        /// The property than, when set to <code>true</code> will cause the recognizer to count up all the tokens in the active list after every frame.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropShowTokenCount = "showTokenCount";

        /// <summary>
        /// The property that controls the number of frames processed for every time
        /// the decode growth step is skipped. Setting this property to zero disables
        /// grow skipping. Setting this number to a small integer will increase the
        /// speed of the decoder but will also decrease its accuracy. The higher the
        /// number, the less often the grow code is skipped. Values like 6-8 is known
        /// to be the good enough for large vocabulary tasks. That means that one of
        /// 6 frames will be skipped.
        /// </summary>
        [S4Integer(DefaultValue = 0)]
        public static string PropGrowSkipInterval = "growSkipInterval";

        /// <summary>
        /// The property that defines the type of active list to use.
        /// </summary>
        [S4Component(Type = typeof(ActiveListManager))]
        public static string PropActiveListManager = "activeListManager";

        /// <summary>
        /// The property for checking if the order of states is valid.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropCheckStateOrder = "checkStateOrder";

        /// <summary>
        /// The property that specifies the maximum lattice edges.
        /// </summary>
        [S4Integer(DefaultValue = 100)]
        public static string PropMaxLatticeEdges = "maxLatticeEdges";

        /// <summary>
        /// The property that controls the amount of simple acoustic lookahead performed. Setting the property to zero (the default) disables simple acoustic lookahead. The lookahead need not be an integer.
        /// </summary>
        [S4Double(DefaultValue = 0)]
        public static string PropAcousticLookaheadFrames = "acousticLookaheadFrames";

        /// <summary>
        /// The property that specifies the relative beam width.
        /// </summary>
        [S4Double(DefaultValue = 0.0)]
        //TODO: this should be a more meaningful default e.g. the common 1E-80
        public static string PropRelativeBeamWidth = "relativeBeamWidth";

        // -----------------------------------
        // Configured Subcomponents
        // -----------------------------------
        protected Linguist.Linguist Linguist; // Provides grammar/language info
        protected IPruner Pruner; // used to prune the active list
        protected IAcousticScorer Scorer; // used to score the active list
        private ActiveListManager _activeListManager;
        private LogMath _logMath;

        // -----------------------------------
        // Configuration data
        // -----------------------------------

        protected Boolean _showTokenCount;
        protected Boolean _checkStateOrder;
        private int _growSkipInterval;
        private float _relativeBeamWidth;
        private float _acousticLookaheadFrames;
        private int _maxLatticeEdges = 100;

        // -----------------------------------
        // Instrumentation
        // -----------------------------------
        protected Timer ScoreTimer;
        protected Timer PruneTimer;
        protected StatisticsVariable TotalTokensScored;
        protected StatisticsVariable CurTokensScored;
        private long _tokenSum;
        private int _tokenCount;

        // -----------------------------------
        // Working data
        // -----------------------------------
        protected HashMap<Object, Token> BestTokenMap;
        protected AlternateHypothesisManager LoserManager;
        private int _numStateOrder;
        // private TokenTracker tokenTracker;
        // private TokenTypeTracker tokenTypeTracker;
        protected Boolean StreamEnd;

        public WordPruningBreadthFirstSearchManager(Linguist.Linguist linguist, IPruner pruner,
                                               IAcousticScorer scorer, ActiveListManager activeListManager,
                                               Boolean showTokenCount, double relativeWordBeamWidth,
                                               int growSkipInterval,
                                               Boolean checkStateOrder, Boolean buildWordLattice,
                                               int maxLatticeEdges, float acousticLookaheadFrames,
                                               Boolean keepAllTokens)
        {

            _logMath = LogMath.GetLogMath();
            Linguist = linguist;
            Pruner = pruner;
            Scorer = scorer;
            _activeListManager = activeListManager;
            _showTokenCount = showTokenCount;
            _growSkipInterval = growSkipInterval;
            _checkStateOrder = checkStateOrder;
            BuildWordLattice = buildWordLattice;
            _maxLatticeEdges = maxLatticeEdges;
            _acousticLookaheadFrames = acousticLookaheadFrames;
            KeepAllTokens = keepAllTokens;

            _relativeBeamWidth = _logMath.LinearToLog(relativeWordBeamWidth);
        }

        public WordPruningBreadthFirstSearchManager()
        {

        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);

            _logMath = LogMath.GetLogMath();

            Linguist = (Linguist.Linguist)ps.GetComponent(PropLinguist);
            Pruner = (IPruner)ps.GetComponent(PropPruner);
            Scorer = (IAcousticScorer)ps.GetComponent(PropScorer);
            _activeListManager = (ActiveListManager)ps.GetComponent(PropActiveListManager);
            _showTokenCount = ps.GetBoolean(PropShowTokenCount);
            _growSkipInterval = ps.GetInt(PropGrowSkipInterval);

            _checkStateOrder = ps.GetBoolean(PropCheckStateOrder);
            _maxLatticeEdges = ps.GetInt(PropMaxLatticeEdges);
            _acousticLookaheadFrames = ps.GetFloat(PropAcousticLookaheadFrames);

            _relativeBeamWidth = _logMath.LinearToLog(ps.GetDouble(PropRelativeBeamWidth));
        }

        public override void Allocate()
        {
            // tokenTracker = new TokenTracker();
            // tokenTypeTracker = new TokenTypeTracker();

            ScoreTimer = TimerPool.GetTimer(this, "Score");
            PruneTimer = TimerPool.GetTimer(this, "Prune");
            GrowTimer = TimerPool.GetTimer(this, "Grow");

            TotalTokensScored = StatisticsVariable.GetStatisticsVariable("totalTokensScored");
            CurTokensScored = StatisticsVariable.GetStatisticsVariable("curTokensScored");
            TokensCreated = StatisticsVariable.GetStatisticsVariable("tokensCreated");

            try
            {
                Linguist.Allocate();
                Pruner.Allocate();
                Scorer.Allocate();
            }
            catch (IOException e)
            {
                throw new SystemException("Allocation of search manager resources failed", e);
            }
        }

        public override void Deallocate()
        {
            try
            {
                Scorer.Deallocate();
                Pruner.Deallocate();
                Linguist.Deallocate();
            }
            catch (IOException e)
            {
                throw new SystemException("Deallocation of search manager resources failed", e);
            }
        }


        /// <summary>
        /// Called at the start of recognition. Gets the search manager ready to recognize
        /// </summary>
        public override void StartRecognition()
        {
            Linguist.StartRecognition();
            Pruner.StartRecognition();
            Scorer.StartRecognition();
            LocalStart();
        }

        /// <summary>
        /// Performs the recognition for the given number of frames.
        /// </summary>
        /// <param name="nFrames">The number of frames to recognize.</param>
        /// <returns>The current result.</returns>
        public override Result Recognize(int nFrames)
        {
            var done = false;
            Result result = null;
            StreamEnd = false;

            for (var i = 0; i < nFrames && !done; i++)
            {
                done = Recognize();
            }

            if (!StreamEnd)
            {
                result = new Result(LoserManager, ActiveList, ResultList, CurrentFrameNumber, done, Linguist.SearchGraph.WordTokenFirst, true);
            }

            // tokenTypeTracker.show();
            if (_showTokenCount)
            {
                ShowTokenCount();
            }
            return result;
        }

        public Boolean Recognize()
        {

            ActiveList = _activeListManager.GetEmittingList();
            var more = ScoreTokens();

            if (more)
            {
                PruneBranches();
                CurrentFrameNumber++;
                this.LogDebug("Current Frame Number: {0}", CurrentFrameNumber);
                if (_growSkipInterval == 0 || (CurrentFrameNumber % _growSkipInterval) != 0)
                {
                    ClearCollectors();
                    GrowEmittingBranches();
                    GrowNonEmittingBranches();
                }
            }
            return !more;
        }

        /// <summary>
        /// Clears lists and maps before next expansion stage.
        /// </summary>
        private void ClearCollectors()
        {
            ResultList = new List<Token>();
            CreateBestTokenMap();
            _activeListManager.ClearEmittingList();
        }


        /// <summary>
        /// Creates a new best token map with the best size.
        /// </summary>
        protected void CreateBestTokenMap()
        {
            var mapSize = ActiveList.Size * 10;
            if (mapSize == 0)
            {
                mapSize = 1;
            }
            BestTokenMap = new HashMap<Object, Token>(mapSize);
            //TODO: The following code has no impact - check usage
            foreach (var iter in BestTokenMap)
                iter.Value.Score = (0.3f);
        }


        /// <summary>
        /// Terminates a recognition.
        /// </summary>
        public override void StopRecognition()
        {
            LocalStop();
            Scorer.StopRecognition();
            Pruner.StopRecognition();
            Linguist.StopRecognition();
        }


        /// <summary>
        /// Gets the initial grammar node from the linguist and creates a GrammarNodeToken.
        /// </summary>
        protected virtual void LocalStart()
        {
            var searchGraph = Linguist.SearchGraph;
            CurrentFrameNumber = 0;
            CurTokensScored.Value = 0;
            _numStateOrder = searchGraph.NumStateOrder;
            _activeListManager.SetNumStateOrder(_numStateOrder);
            if (BuildWordLattice)
            {
                LoserManager = new AlternateHypothesisManager(_maxLatticeEdges);
            }

            var state = searchGraph.InitialState;

            ActiveList = _activeListManager.GetEmittingList();
            ActiveList.Add(new Token(state, CurrentFrameNumber));

            ClearCollectors();

            GrowBranches();
            GrowNonEmittingBranches();
            // tokenTracker.setEnabled(false);
            // tokenTracker.startUtterance();
        }


        /// <summary>
        /// Local cleanup for this search manager.
        /// </summary>
        protected void LocalStop()
        {
            // tokenTracker.stopUtterance();
        }

        /// <summary>
        /// Goes through the active list of tokens and expands each token, 
        /// finding the set of successor tokens until all the successor tokens are emitting tokens.
        /// </summary>
        protected void GrowBranches()
        {
            GrowTimer.Start();
            var relativeBeamThreshold = ActiveList.GetBeamThreshold();
            //this.LogInfo("Frame: " + currentFrameNumber
            //            + " thresh : " + relativeBeamThreshold + " bs "
            //            + activeList.getBestScore() + " tok "
            //            + activeList.getBestToken());
            this.LogDebug("RelativeBeamThreshold: {0}", relativeBeamThreshold.ToString("R"));
            var tokenList = ActiveList;
            foreach (var token in tokenList)
            {
                if (token == null) break;
                if (token.Score >= relativeBeamThreshold && AllowExpansion(token))
                {
                    CollectSuccessorTokens(token);
                }
            }


            //this.LogDebug(string.Format("ActiveList:{0} ",activeList.Count()));
            GrowTimer.Stop();
        }


        /// <summary>
        /// Grows the emitting branches. This version applies a simple acoustic lookahead based upon the rate of change in the current acoustic score.
        /// </summary>
        protected void GrowEmittingBranches()
        {
            if (_acousticLookaheadFrames > 0F)
            {
                GrowTimer.Start();
                var bestScore = -Float.MAX_VALUE;
                foreach (var t in ActiveList)
                {
                    var score = t.Score + t.AcousticScore * _acousticLookaheadFrames;
                    if (score > bestScore)
                    {
                        bestScore = score;
                    }
                    //t.setWorkingScore(score);
                }
                var relativeBeamThreshold = bestScore + _relativeBeamWidth;
                this.LogDebug("RelativeBeamThreshold: {0}", relativeBeamThreshold.ToString("R"));

                foreach (var t in ActiveList)
                {
                    if (t.Score + t.AcousticScore * _acousticLookaheadFrames > relativeBeamThreshold)
                    {
                        CollectSuccessorTokens(t);
                    }
                }
                GrowTimer.Stop();
            }
            else
            {
                GrowBranches();
            }
        }


        /// <summary>
        /// Grow the non-emitting branches, until the tokens reach an emitting state.
        /// </summary>
        private void GrowNonEmittingBranches()
        {
            for (var i = _activeListManager.GetNonEmittingListIterator(); i.MoveNext(); )
            {
                ActiveList = i.Current;
                if (ActiveList != null)
                {
                    i.Remove();
                    PruneBranches();
                    GrowBranches();
                } 
            }
        }

        /// <summary>
        /// Calculate the acoustic scores for the active list. The active list should contain only emitting tokens.
        /// </summary>
        /// <returns><code>true</code> if there are more frames to score, otherwise, false</returns>
        protected bool ScoreTokens()
        {
            ScoreTimer.Start();
            var data = Scorer.CalculateScores(ActiveList.GetTokens());
            this.LogDebug("Scored Data: {0}", data);
            ScoreTimer.Stop();

            Token bestToken = null;
            if (data is Token)
            {
                bestToken = (Token)data;
            }
            else if (data == null)
            {
                StreamEnd = true;
            }

            var moreTokens = (bestToken != null);
            ActiveList.SetBestToken(bestToken);

            //monitorWords(activeList);
            MonitorStates(ActiveList);

            this.LogDebug("bestToken: {0}", bestToken);

            CurTokensScored.Value += ActiveList.Size;
            TotalTokensScored.Value += ActiveList.Size;

            return moreTokens;
        }

        /// <summary>
        /// Keeps track of and reports all of the active word histories for the given active list.
        /// </summary>
        /// <param name="activeList">The active list to track.</param>
        private void MonitorWords(ActiveList activeList)
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

        /// <summary>
        /// Keeps track of and reports statistics about the number of active states.
        /// </summary>
        /// <param name="activeList">The active list of states.</param>
        protected void MonitorStates(ActiveList activeList)
        {

            _tokenSum += activeList.Size;
            _tokenCount++;

            if ((_tokenCount % 1000) == 0)
            {
                this.LogInfo("Average Tokens/State: " + (_tokenSum / _tokenCount));
            }
        }


        /// <summary>
        /// Removes unpromising branches from the active list.
        /// </summary>
        protected void PruneBranches()
        {
            PruneTimer.Start();
            ActiveList = Pruner.Prune(ActiveList);
            PruneTimer.Stop();
        }

        /// <summary>
        /// Gets the best token for this state.
        /// </summary>
        /// <param name="state">The state of interest.</param>
        /// <returns>The best token.</returns>
        protected Token GetBestToken(ISearchState state)
        {
            return BestTokenMap.Get(state);//[key];
        }


        /**
        /// Sets the best token for a given state
         *
        /// @param token the best token
        /// @param state the state
         */
        protected void SetBestToken(Token token, ISearchState state)
        {
            BestTokenMap.Add(state, token);
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
 


        /** Checks that the given two states are in legitimate order.
        /// @param fromState
        /// @param toState*/

        protected void CheckStateOrder(ISearchState fromState, ISearchState toState)
        {
            if (fromState.Order == _numStateOrder - 1)
            {
                return;
            }

            if (fromState.Order > toState.Order)
            {
                throw new Exception("IllegalState order: from "
                        + fromState.GetType().Name + ' '
                        + fromState.ToPrettyString()
                        + " order: " + fromState.Order
                        + " to "
                        + toState.GetType().Name + ' '
                        + toState.ToPrettyString()
                        + " order: " + toState.Order);
            }
        }


        /**
        /// Collects the next set of emitting tokens from a token and accumulates them in the active or result lists
         *
        /// @param token the token to collect successors from be immediately expanded are placed. Null if we should always
        ///              expand all nodes.
         */
        protected virtual void CollectSuccessorTokens(Token token)
        {

            // tokenTracker.add(token);
            // tokenTypeTracker.add(token);

            // If this is a final state, add it to the final list

            if (token.IsFinal)
            {
                ResultList.Add(GetResultListPredecessor(token));
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

            if (!token.IsEmitting && (KeepAllTokens && IsVisited(token)))
            {
                return;
            }

            var state = token.SearchState;
            var arcs = state.GetSuccessors();
            //this.LogDebug("Total Arcs: {0}", arcs.Length);
            var predecessor = GetResultListPredecessor(token);

            // For each successor
            // calculate the entry score for the token based upon the
            // predecessor token score and the transition probabilities
            // if the score is better than the best score encountered for
            // the SearchState and frame then create a new token, add
            // it to the lattice and the SearchState.
            // If the token is an emitting token add it to the list,
            // otherwise recursively collect the new tokens successors.

            foreach (var arc in arcs)
            {
                var nextState = arc.State;
                //this.LogDebug("NextState is of type: {0}",nextState.GetType());

                if (_checkStateOrder)
                {
                    CheckStateOrder(state, nextState);
                }

                // We're actually multiplying the variables, but since
                // these come in log(), multiply gets converted to add
                var logEntryScore = token.Score + arc.GetProbability();//TODO: CHECK

                var bestToken = GetBestToken(nextState);

                //      

                if (bestToken == null)
                {
                    var newBestToken = new Token(predecessor, nextState, logEntryScore, arc.InsertionProbability, arc.LanguageProbability, CurrentFrameNumber);
                    TokensCreated.Value++;
                    SetBestToken(newBestToken, nextState);
                    ActiveListAdd(newBestToken);
                }
                else if (bestToken.Score < logEntryScore)
                {
                    // System.out.println("Updating " + bestToken + " with " +
                    // newBestToken);
                    var oldPredecessor = bestToken.Predecessor;
                    bestToken.Update(predecessor, nextState, logEntryScore, arc.InsertionProbability, arc.LanguageProbability, CurrentFrameNumber);
                    if (BuildWordLattice && nextState is IWordSearchState)
                    {
                        LoserManager.AddAlternatePredecessor(bestToken, oldPredecessor);
                    }
                }
                else if (BuildWordLattice && nextState is IWordSearchState)
                {
                    if (predecessor != null)
                    {
                        LoserManager.AddAlternatePredecessor(bestToken, predecessor);
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

        protected Boolean IsVisited(Token t)
        {
            var curState = t.SearchState;

            t = t.Predecessor;

            while (t != null && !t.IsEmitting)
            {
                if (curState.Equals(t.SearchState))
                {
                    this.LogInfo("CS " + curState + " match " + t.SearchState);
                    return true;
                }
                t = t.Predecessor;
            }
            return false;
        }


        protected void ActiveListAdd(Token token)
        {
            _activeListManager.Add(token);
        }


        /// <summary>
        /// Determine if the given token should be expanded.
        /// </summary>
        /// <param name="t">The token to test.</param>
        /// <returns><code>true</code> if the token should be expanded</returns>
        protected Boolean AllowExpansion(Token t)
        {
            return true; // currently disabled
        }

        /// <summary>
        /// Counts all the tokens in the active list (and displays them). This is an expensive operation.
        /// </summary>
        protected void ShowTokenCount()
        {
            var tokenSet = new HashSet<Token>();

            foreach (var token in ActiveList)
            {
                while (token != null)
                {
                    tokenSet.Add(token);
                    //TODO: CHECK SEMANTICS
                    //token = token.getPredecessor();
                }
            }

            this.LogInfo("Token Lattice size: " + tokenSet.Count);

            tokenSet = new HashSet<Token>();

            foreach (var token in ResultList)
            {
                while (token != null)
                {
                    tokenSet.Add(token);
                    //token = token.getPredecessor();
                }
            }

            this.LogInfo("Result Lattice size: " + tokenSet.Count);
        }


        /// <summary>
        /// Gets or sets the active list.
        /// </summary>
        /// <value>
        /// The active list.
        /// </value>
        public ActiveList ActiveList { get; set; }

        /// <summary>
        /// Gets or sets the result list.
        /// </summary>
        /// <value>
        /// The result list.
        /// </value>
        public List<Token> ResultList { get; set; }

        /// <summary>
        /// Gets or sets the current frame number.
        /// </summary>
        /// <value>
        /// The current frame number.
        /// </value>
        public int CurrentFrameNumber { get; protected set; }

        /// <summary>
        /// Gets or sets the grow timer.
        /// </summary>
        /// <value>
        /// The grow timer.
        /// </value>
        public Timer GrowTimer { get; protected set; }


        /// <summary>
        /// Returns the tokensCreated StatisticsVariable.
        /// </summary>
        /// <value>
        /// The tokens created.
        /// </value>
        public StatisticsVariable TokensCreated { get; protected set; }
    }
}
