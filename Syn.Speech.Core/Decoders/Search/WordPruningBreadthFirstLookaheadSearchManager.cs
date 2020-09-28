using System;
using System.Collections.Generic;
using Syn.Speech.Decoders.Pruner;
using Syn.Speech.Decoders.Scorer;
using Syn.Speech.Helper;
using Syn.Speech.Linguist;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
using Syn.Speech.Linguist.Allphone;
using Syn.Speech.Linguist.LexTree;
using Syn.Speech.Results;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    /// <summary>
    /// Provides the breadth first search with fast match heuristic included to reduce amount of tokens created. 
    /// All scores and probabilities are maintained in the log math log domain.
    /// </summary>
    public class WordPruningBreadthFirstLookaheadSearchManager : WordPruningBreadthFirstSearchManager
    {
        /// <summary>
        /// The property that to get direct access to gau for score caching control. 
        /// </summary>
        [S4Component(Type = typeof(ILoader))]
        public const String PropLoader = "loader";

        /// <summary>
        /// The property that defines the name of the linguist to be used for fast match.
        /// </summary>
        [S4Component(Type = typeof(Linguist.Linguist))]
        public const String PropFastmatchLinguist = "fastmatchLinguist";

        /// <summary>
        /// The property that defines the type active list factory for fast match
        /// </summary>
        [S4Component(Type = typeof(ActiveListFactory))]
        public const String PropFmActiveListFactory = "fastmatchActiveListFactory";

        [S4Double(DefaultValue = 1.0)]
        public const String PropLookaheadPenaltyWeight = "lookaheadPenaltyWeight";

        /// <summary>
        /// The property that controls size of lookahead window. Acceptable values are in range [1..10].
        /// </summary>
        [S4Integer(DefaultValue = 5)]
        public const String PropLookaheadWindow = "lookaheadWindow";

        // -----------------------------------
        // Configured Subcomponents
        // -----------------------------------
        private Linguist.Linguist _fastmatchLinguist; // Provides phones info for fastmatch
        private ILoader _loader;
        private ActiveListFactory _fastmatchActiveListFactory;

        // -----------------------------------
        // Lookahead data
        // -----------------------------------
        private int _lookaheadWindow;
        private float _lookaheadWeight;
        private HashMap<Integer, Float> _penalties;
        private LinkedList<FrameCiScores> _ciScores;

        // -----------------------------------
        // Working data
        // -----------------------------------
        private int _currentFastMatchFrameNumber; // the current frame number for
        // lookahead matching
        protected ActiveList FastmatchActiveList; // the list of active tokens for
        // fast match
        protected HashMap<ISearchState, Token> FastMatchBestTokenMap;
        private bool _fastmatchStreamEnd;

        public WordPruningBreadthFirstLookaheadSearchManager(Linguist.Linguist linguist, Linguist.Linguist fastmatchLinguist, ILoader loader,
                IPruner pruner, IAcousticScorer scorer, ActiveListManager activeListManager,
                ActiveListFactory fastmatchActiveListFactory, bool showTokenCount, double relativeWordBeamWidth,
                int growSkipInterval, bool checkStateOrder, bool buildWordLattice, int lookaheadWindow, float lookaheadWeight,
                int maxLatticeEdges, float acousticLookaheadFrames, bool keepAllTokens)
            : base(linguist, pruner, scorer, activeListManager, showTokenCount, relativeWordBeamWidth, growSkipInterval,
                checkStateOrder, buildWordLattice, maxLatticeEdges, acousticLookaheadFrames, keepAllTokens)
        {
            _loader = loader;
            _fastmatchLinguist = fastmatchLinguist;
            _fastmatchActiveListFactory = fastmatchActiveListFactory;
            _lookaheadWindow = lookaheadWindow;
            _lookaheadWeight = lookaheadWeight;
            if (lookaheadWindow < 1 || lookaheadWindow > 10)
                throw new ArgumentException("Unsupported lookahead window size: " + lookaheadWindow
                        + ". Value in range [1..10] is expected");
            _ciScores = new LinkedList<FrameCiScores>();
            _penalties = new HashMap<Integer, Float>();
            if (loader is Sphinx3Loader && ((Sphinx3Loader)loader).HasTiedMixtures()) ((Sphinx3Loader)loader).SetGauScoresQueueLength(lookaheadWindow + 2);
        }

        public WordPruningBreadthFirstLookaheadSearchManager()
        {

        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);

            _fastmatchLinguist = (Linguist.Linguist)ps.GetComponent(PropFastmatchLinguist);
            _fastmatchActiveListFactory = (ActiveListFactory)ps.GetComponent(PropFmActiveListFactory);
            _loader = (ILoader)ps.GetComponent(PropLoader);
            _lookaheadWindow = ps.GetInt(PropLookaheadWindow);
            _lookaheadWeight = ps.GetFloat(PropLookaheadPenaltyWeight);
            if (_lookaheadWindow < 1 || _lookaheadWindow > 10)
                throw new PropertyException(typeof(WordPruningBreadthFirstLookaheadSearchManager).Name, PropLookaheadWindow,
                        "Unsupported lookahead window size: " + _lookaheadWindow + ". Value in range [1..10] is expected");
            _ciScores = new LinkedList<FrameCiScores>();
            _penalties = new HashMap<Integer, Float>();
            if (_loader is Sphinx3Loader && ((Sphinx3Loader) _loader).HasTiedMixtures())
            {
                ((Sphinx3Loader)_loader).SetGauScoresQueueLength(_lookaheadWindow + 2);
            }
        }

        /// <summary>
        /// Performs the recognition for the given number of frames.
        /// </summary>
        /// <param name="nFrames">The number of frames to recognize.</param>
        /// <returns>
        /// The current result.
        /// </returns>
        public override Result Recognize(int nFrames)
        {
            var done = false;
            Result result = null;
            StreamEnd = false;

            for (var i = 0; i < nFrames && !done; i++)
            {
                if (!_fastmatchStreamEnd)
                {
                    FastMatchRecognize();
                }
                _penalties.Clear();
                _ciScores.Poll();
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

        private void FastMatchRecognize()
        {
            var more = ScoreFastMatchTokens();

            if (more)
            {
                PruneFastMatchBranches();
                _currentFastMatchFrameNumber++;
                CreateFastMatchBestTokenMap();
                GrowFastmatchBranches();
            }
        }

        /// <summary>
        /// creates a new best token map with the best size
        /// </summary>
        protected void CreateFastMatchBestTokenMap()
        {
            var mapSize = FastmatchActiveList.Size * 10;
            if (mapSize == 0)
            {
                mapSize = 1;
            }
            FastMatchBestTokenMap = new HashMap<ISearchState, Token>(mapSize);
        }

        /// <summary>
        /// Gets the initial grammar node from the linguist and creates a GrammarNodeToken
        /// </summary>
        protected override void LocalStart()
        {
            _currentFastMatchFrameNumber = 0;
            if (_loader is Sphinx3Loader && ((Sphinx3Loader) _loader).HasTiedMixtures())
            {
                ((Sphinx3Loader)_loader).ClearGauScores();
            }
            // prepare fast match active list
            FastmatchActiveList = _fastmatchActiveListFactory.NewInstance();
            var fmInitState = _fastmatchLinguist.SearchGraph.InitialState;
            FastmatchActiveList.Add(new Token(fmInitState, _currentFastMatchFrameNumber));
            CreateFastMatchBestTokenMap();
            GrowFastmatchBranches();
            _fastmatchStreamEnd = false;
            for (var i = 0; (i < _lookaheadWindow - 1) && !_fastmatchStreamEnd; i++)
            {
                FastMatchRecognize();
            }
            base.LocalStart();
        }

        /// <summary>
        /// Goes through the fast match active list of tokens and expands each token, 
        /// finding the set of successor tokens until all the successor tokens are emitting tokens.
        /// </summary>
        protected void GrowFastmatchBranches()
        {
            GrowTimer.Start();
            var oldActiveList = FastmatchActiveList;
            FastmatchActiveList = _fastmatchActiveListFactory.NewInstance();
            var fastmathThreshold = oldActiveList.GetBeamThreshold();
            // TODO more precise range of baseIds, remove magic number
            var frameCiScores = new float[100];

            Arrays.Fill(frameCiScores, -Float.MAX_VALUE);
            var frameMaxCiScore = -Float.MAX_VALUE;
            foreach (var token in oldActiveList)
            {
                var tokenScore = token.Score;
                if (tokenScore < fastmathThreshold) { continue; }
                    
                // filling max ci scores array that will be used in general search
                // token score composing
                if (token.SearchState is PhoneHmmSearchState)
                {
                    var baseId = ((PhoneHmmSearchState)token.SearchState).GetBaseId();
                    if (frameCiScores[baseId] < tokenScore)
                        frameCiScores[baseId] = tokenScore;
                    if (frameMaxCiScore < tokenScore)
                        frameMaxCiScore = tokenScore;
                }
                CollectFastMatchSuccessorTokens(token);
            }
            _ciScores.Add(new FrameCiScores(frameCiScores, frameMaxCiScore));
            GrowTimer.Stop();
        }

        protected bool ScoreFastMatchTokens()
        {
            ScoreTimer.Start();
            var data = Scorer.CalculateScoresAndStoreData(FastmatchActiveList.GetTokens());
            ScoreTimer.Stop();

            Token bestToken = null;
            if (data is Token)
            {
                bestToken = (Token)data;
            }
            else
            {
                _fastmatchStreamEnd = true;
            }

            var moreTokens = (bestToken != null);
            FastmatchActiveList.SetBestToken(bestToken);

            // monitorWords(activeList);
            MonitorStates(FastmatchActiveList);

            // System.out.println("BEST " + bestToken);

            CurTokensScored.Value += FastmatchActiveList.Size;
            TotalTokensScored.Value += FastmatchActiveList.Size;

            return moreTokens;
        }

        /// <summary>
        /// Removes unpromising branches from the fast match active list.
        /// </summary>
        protected void PruneFastMatchBranches()
        {
            PruneTimer.Start();
            FastmatchActiveList = Pruner.Prune(FastmatchActiveList);
            PruneTimer.Stop();
        }

        protected Token GetFastMatchBestToken(ISearchState state)
        {
            return FastMatchBestTokenMap.Get(state);
        }

        protected void SetFastMatchBestToken(Token token, ISearchState state)
        {
            FastMatchBestTokenMap.Put(state, token);
        }

        protected void CollectFastMatchSuccessorTokens(Token token)
        {
            var state = token.SearchState;
            var arcs = state.GetSuccessors();
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
                // We're actually multiplying the variables, but since
                // these come in log(), multiply gets converted to add
                var logEntryScore = token.Score + arc.GetProbability();
                var predecessor = GetResultListPredecessor(token);

                // if not emitting, check to see if we've already visited
                // this state during this frame. Expand the token only if we
                // haven't visited it already. This prevents the search
                // from getting stuck in a loop of states with no
                // intervening emitting nodes. This can happen with nasty
                // jsgf grammars such as ((foo*)*)*
                if (!nextState.IsEmitting)
                {
                    var newToken = new Token(predecessor, nextState, logEntryScore, arc.InsertionProbability, arc.LanguageProbability, _currentFastMatchFrameNumber);
                    TokensCreated.Value++;
                    if (!IsVisited(newToken))
                    {
                        CollectFastMatchSuccessorTokens(newToken);
                    }
                    continue;
                }

                var bestToken = GetFastMatchBestToken(nextState);
                if (bestToken == null)
                {
                    var newToken = new Token(predecessor, nextState, logEntryScore, arc.InsertionProbability, arc.LanguageProbability, _currentFastMatchFrameNumber);
                    TokensCreated.Value++;
                    SetFastMatchBestToken(newToken, nextState);
                    FastmatchActiveList.Add(newToken);
                }
                else
                {
                    if (bestToken.Score <= logEntryScore)
                    {
                        bestToken.Update(predecessor, nextState, logEntryScore, arc.InsertionProbability,
                                arc.LanguageProbability, _currentFastMatchFrameNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Collects the next set of emitting tokens from a token and accumulates them in the active or result lists
        /// </summary>
        /// <param name="token">The token to collect successors from be immediately expanded are placed. Null if we should always expand all nodes.</param>
        protected override void CollectSuccessorTokens(Token token)
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
            var predecessor = GetResultListPredecessor(token);

            // For each successor
            // calculate the entry score for the token based upon the
            // predecessor token score and the transition probabilities
            // if the score is better than the best score encountered for
            // the SearchState and frame then create a new token, add
            // it to the lattice and the SearchState.
            // If the token is an emitting token add it to the list,
            // otherwise recursively collect the new tokens successors.

            var tokenScore = token.Score;
            var beamThreshold = ActiveList.GetBeamThreshold();
            var stateProducesPhoneHmms = state is LexTreeNonEmittingHMMState || state is LexTreeWordState
                    || state is LexTreeEndUnitState;
            foreach (var arc in arcs)
            {
                var nextState = arc.State;

                // prune states using lookahead heuristics
                if (stateProducesPhoneHmms)
                {
                    if (nextState is LexTreeHmmState)
                    {
                        Float penalty;
                        var baseId = ((LexTreeHmmState)nextState).HmmState.HMM.BaseUnit.BaseID;
                        if ((penalty = _penalties.Get(baseId)) == null)
                            penalty = UpdateLookaheadPenalty(baseId);
                        if ((tokenScore + _lookaheadWeight * penalty) < beamThreshold)
                            continue;
                    }
                }

                if (_checkStateOrder)
                {
                    CheckStateOrder(state, nextState);
                }

                // We're actually multiplying the variables, but since
                // these come in log(), multiply gets converted to add
                var logEntryScore = tokenScore + arc.GetProbability();

                var bestToken = GetBestToken(nextState);

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

        private Float UpdateLookaheadPenalty(int baseId)
        {
            if (_ciScores.IsEmpty())
            {
                return 0.0f;
            }
            var penalty = -Float.MAX_VALUE;
            foreach (var frameCiScores in _ciScores)
            {
                var diff = frameCiScores.Scores[baseId] - frameCiScores.MaxScore;
                if (diff > penalty)
                {
                    penalty = diff;
                }
            }
            _penalties.Put(baseId, penalty);
            return penalty;
        }

        private class FrameCiScores
        {
            public readonly float[] Scores;
            public readonly float MaxScore;

            public FrameCiScores(float[] scores, float maxScore)
            {
                Scores = scores;
                MaxScore = maxScore;
            }
        }
    }
}
