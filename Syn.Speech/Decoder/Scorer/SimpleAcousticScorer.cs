using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Common.FrontEnd;
using Syn.Speech.Decoder.Search;
using Syn.Speech.FrontEnd;
using Syn.Speech.FrontEnd.EndPoint;
using Syn.Speech.FrontEnd.Util;
using Syn.Speech.Util.Props;

//PATROLLED
namespace Syn.Speech.Decoder.Scorer
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
    public class SimpleAcousticScorer : IAcousticScorer
    {
        /** Property the defines the frontend to retrieve features from for scoring */
        [S4Component(type = typeof(BaseDataProcessor))]
        public static String FEATURE_FRONTEND = "frontend";
        protected BaseDataProcessor frontEnd;

        /**
        /// An optional post-processor for computed scores that will normalize scores. If not set, no normalization will
        /// applied and the token scores will be returned unchanged.
         */
        [S4Component(type = typeof(IScoreNormalizer), mandatory = false)]
        public static String SCORE_NORMALIZER = "scoreNormalizer";
        private IScoreNormalizer scoreNormalizer;

        private Boolean useSpeechSignals;

        void IConfigurable.newProperties(PropertySheet ps)
        {
 	        newProperties(ps);
        }

        
        virtual public void newProperties(PropertySheet ps)
        {
            ///base.newProperties(ps);
            ///not mandatory

            this.frontEnd = (BaseDataProcessor) ps.getComponent(FEATURE_FRONTEND);
            this.scoreNormalizer = (IScoreNormalizer) ps.getComponent(SCORE_NORMALIZER);
        }

        /**
        /// @param frontEnd the frontend to retrieve features from for scoring
        /// @param scoreNormalizer optional post-processor for computed scores that will normalize scores. If not set, no normalization will
        /// applied and the token scores will be returned unchanged.
         */
        public SimpleAcousticScorer(BaseDataProcessor frontEnd, IScoreNormalizer scoreNormalizer) 
        {
            this.frontEnd = frontEnd;
            this.scoreNormalizer = scoreNormalizer;
        }

        public SimpleAcousticScorer() 
        {
        }

        /// <summary>
        /// Scores the given set of states.
        /// </summary>
        /// <param name="scoreableList">A list containing scoreable objects to be scored</param>
        /// <returns>The best scoring scoreable, or <code>null</code> if there are no more features to score</returns>
        public IData calculateScores(List<IScoreable> scoreableList) 
        {

    	    try 
            {
                IData data;
                while ((data = getNextData()) is Signal) 
                {
                    if (data is SpeechEndSignal || data is DataEndSignal) 
                        return data;
                    
                }

                if (data == null || scoreableList.Count==0)
            	    return null;
            
                // convert the data to FloatData if not yet done
                if (data is DoubleData)
                    data = DataUtil.DoubleData2FloatData((DoubleData) data);

                IScoreable bestToken = doScoring(scoreableList, data);

                // apply optional score normalization
                if (scoreNormalizer != null && bestToken is Token)
                    bestToken = scoreNormalizer.normalize(scoreableList, bestToken);

                return bestToken;
            } 
            catch (Exception e) 
            {
                //e.printStackTrace();
                Trace.WriteLine(e.Message);
                return null;
            }
        }

        private IData getNextData() 
        {
            IData data = frontEnd.getData();

            // reconfigure the scorer for the coming data stream
            if (data is DataStartSignal)
                handleDataStartSignal((DataStartSignal)data);
            if (data is DataEndSignal)
                handleDataEndSignal((DataEndSignal)data);

            return data;
        }

        /** Handles the first element in a feature-stream.
        /// @param dataStartSignal*/
        protected void handleDataStartSignal(DataStartSignal dataStartSignal) 
        {
            Dictionary<String, Object> dataProps = dataStartSignal.getProps();

            useSpeechSignals = dataProps.ContainsKey(DataStartSignal.SPEECH_TAGGED_FEATURE_STREAM) && (Boolean) dataProps[DataStartSignal.SPEECH_TAGGED_FEATURE_STREAM];
        }

        /** Handles the last element in a feature-stream.
        /// @param dataEndSignal*/
        protected void handleDataEndSignal(DataEndSignal dataEndSignal) 
        {
            // we don't treat the end-signal here, but extending classes might do
        }

        public void startRecognition() 
        {
            if (!useSpeechSignals) 
            {
                IData firstData = getNextData();
                if (firstData == null)
        	        return;
            
                Trace.Assert(firstData is DataStartSignal,
                        "The first element in an sphinx4-feature stream must be a DataStartSignal but was a " + firstData.GetType().Name);
            }

            if (!useSpeechSignals)
                return;

            IData data;
            while (!((data = getNextData()) is SpeechStartSignal)) 
            {
                if (data == null) 
                {
                    break;
                }
            }
        }

        public void stopRecognition() 
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
        protected IScoreable doScoring(List<IScoreable> scoreableList, IData data)
        {
            IEnumerator<IScoreable> i = scoreableList.GetEnumerator();
            IScoreable best = i.Current;
            best.calculateScore(data);
            while (i.MoveNext()) 
            {
                IScoreable scoreable = i.Current;
                if (scoreable.calculateScore(data) > best.getScore())
                    best = scoreable;
            }
            return best;
        }

        // Even if we don't do any meaningful allocation here, we implement the methods because
        // most extending scorers do need them either.
    
        public void allocate() 
        {
        }

        public void deallocate() 
        {
        }

    }
}
