using System.Collections.Generic;
using Syn.Speech.Decoders.Search;
using Syn.Speech.FrontEnds;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.FrontEnds.Util;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Scorer
{
    /// <summary>
    /// Implements some basic scorer functionality, including a simple default
    /// acoustic scoring implementation which scores within the current thread,
    /// that can be changed by overriding the {@link #doScoring} method.
    ///
    /// Note that all scores are maintained in LogMath log base.
    ///
    /// @author Holger Brandl
    /// </summary>
    public class SimpleAcousticScorer : ConfigurableAdapter, IAcousticScorer
    {
        /// <summary>
        ///  Property the defines the frontend to retrieve features from for scoring
        /// </summary>
        [S4Component(Type = typeof(BaseDataProcessor))]
        public static string FeatureFrontend = "frontend";
        protected BaseDataProcessor FrontEnd;

        /**
        /// An optional post-processor for computed scores that will normalize scores. If not set, no normalization will
        /// applied and the token scores will be returned unchanged.
         */
        [S4Component(Type = typeof(IScoreNormalizer), Mandatory = false)]
        public static string ScoreNormalizer = "scoreNormalizer";
        private IScoreNormalizer _scoreNormalizer;

        private LinkedList<IData> _storedData;
        private bool _seenEnd;


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            FrontEnd = (BaseDataProcessor)ps.GetComponent(FeatureFrontend);
            _scoreNormalizer = (IScoreNormalizer)ps.GetComponent(ScoreNormalizer);
            _storedData = new LinkedList<IData>();
        }

        /**
        /// @param frontEnd the frontend to retrieve features from for scoring
        /// @param scoreNormalizer optional post-processor for computed scores that will normalize scores. If not set, no normalization will
        /// applied and the token scores will be returned unchanged.
         */
        public SimpleAcousticScorer(BaseDataProcessor frontEnd, IScoreNormalizer scoreNormalizer)
        {
            FrontEnd = frontEnd;
            _scoreNormalizer = scoreNormalizer;
            _storedData = new LinkedList<IData>();
        }

        public SimpleAcousticScorer()
        {
        }

        /// <summary>
        /// Scores the given set of states.
        /// </summary>
        /// <param name="scoreableList">A list containing scoreable objects to be scored</param>
        /// <returns>The best scoring scoreable, or <code>null</code> if there are no more features to score</returns>
        public IData CalculateScores<T>(List<T> scoreableList) where T : IScoreable
        {
            IData data;
            if (_storedData.IsEmpty())
            {
                while ((data = GetNextData()) is Signal)
                {
                    if (data is SpeechEndSignal)
                    {
                        _seenEnd = true;
                        break;
                    }
                    if (data is DataEndSignal)
                    {
                        if (_seenEnd) return null;
                        break;
                    }
                }
                if (data == null) return null;
            }
            else
            {
                data = _storedData.Poll();
            }

            return CalculateScoresForData(scoreableList, data);
        }

        public IData CalculateScoresAndStoreData<T>(List<T> scoreableList) where T : IScoreable
        {
            IData data;
            while ((data = GetNextData()) is Signal)
            {
                if (data is SpeechEndSignal)
                {
                    _seenEnd = true;
                    break;
                }
                if (data is DataEndSignal)
                {
                    if (_seenEnd)
                        return null;
                    break;
                }
            }
            if (data == null)
                return null;

            _storedData.Add(data);

            return CalculateScoresForData(scoreableList, data);
        }


        protected IData CalculateScoresForData<T>(List<T> scoreableList, IData data) where T : IScoreable
        {
            if (data is SpeechEndSignal || data is DataEndSignal)
            {
                return data;
            }

            if (scoreableList.IsEmpty())
                return null;

            // convert the data to FloatData if not yet done
            if (data is DoubleData)
                data = DataUtil.DoubleData2FloatData((DoubleData)data);

            IScoreable bestToken = DoScoring(scoreableList, data);

            // apply optional score normalization
            if (_scoreNormalizer != null && bestToken is Token)
                bestToken = _scoreNormalizer.Normalize(scoreableList, bestToken);

            return bestToken;
        }

        private IData GetNextData()
        {
            IData data = FrontEnd.GetData();
            return data;
        }

        public void StartRecognition()
        {
            _storedData.Clear();
        }

        public void StopRecognition()
        {
            // nothing needs to be done here
        }

        /**
        /// Scores a a list of <code>Scoreable</code>s given a <code>Data</code>-object.
         *
        /// @param scoreableList The list of Scoreables to be scored
        /// @param data          The <code>Data</code>-object to be used for scoring.
        /// @return the best scoring <code>Scoreable</code> or <code>null</code> if the list of scoreables was empty.
        /// @throws Exception 
         */

        protected virtual IScoreable DoScoring<T>(List<T> scoreableList, IData data) where T : IScoreable
        {
            T best = default(T);
            float bestScore = -Float.MAX_VALUE;
            foreach (T item in scoreableList)
            {
                item.CalculateScore(data);
                if (item.Score > bestScore)
                {
                    bestScore = item.Score;
                    best = item;
                }
            }
            return best;
        }

        // Even if we don't do any meaningful allocation here, we implement the methods because
        // most extending scorers do need them either.

        public virtual void Allocate()
        {
        }

        public virtual void Deallocate()
        {
        }

    }
}
