using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.Decoders.Scorer;
using Syn.Speech.Decoders.Search;
using Syn.Speech.FrontEnds;
using Syn.Speech.FrontEnds.DataBranch;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Helper;
using Syn.Speech.Linguist;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.Decoder
{
    [TestClass]
    public class ScorerTests
    {
        #region TestToken

        readonly TestToken _testToken = new TestToken(null, 0f, 0f, 0f, 0f);
        public class TestToken : Token
        {
            public TestToken(Token predecessor, ISearchState state, float logTotalScore, float logInsertionScore, float logLanguageScore, int frameNumber)
                : base(predecessor, state, logTotalScore, logInsertionScore, logLanguageScore, frameNumber)
            {

            }

            public TestToken(ISearchState state, int frameNumber)
                : base(state, frameNumber)
            {

            }

            public TestToken(Token predecessor, float logTotalScore, float logAcousticScore, float logInsertionScore, float logLanguageScore)
                : base(predecessor, logTotalScore, logAcousticScore, logInsertionScore, logLanguageScore)
            {

            }

            public override float CalculateScore(IData feature)
            {
                return -1;
            }
        }
        #endregion

        [TestMethod]
        public void Scorer_WaitUntilSpeechStart()
        {
            var scorerClasses = new List<Type> { typeof(SimpleAcousticScorer), typeof(ThreadedAcousticScorer) };

            foreach (var scorerClass in scorerClasses)
            {
                var dummyFrontEnd = CreateDummyFrontEnd();

                var props = new HashMap<String, Object>();
                props.Put(SimpleAcousticScorer.FeatureFrontend, dummyFrontEnd);
                var scorer = ConfigurationManager.GetInstance<SimpleAcousticScorer>(scorerClass, props);

                var startBufferSize = dummyFrontEnd.BufferSize;

                scorer.Allocate();
                scorer.StartRecognition();

                var dummyTokens = Arrays.AsList(_testToken);

                for (var i = 0; i < 100; i++)
                    scorer.CalculateScores(dummyTokens);

                Assert.IsTrue(dummyFrontEnd.BufferSize < (startBufferSize - 100));

                scorer.StopRecognition();
                scorer.Deallocate();
            }
        }

        private static DataBufferProcessor CreateDummyFrontEnd()
        {
            var bufferProc = ConfigurationManager.GetInstance<DataBufferProcessor>();
            bufferProc.ProcessDataFrame(new DataStartSignal(16000));

            foreach (var doubleData in RandomDataProcessor.CreateFeatVectors(5, 16000, 0, 39, 10))
                bufferProc.ProcessDataFrame(doubleData);

            bufferProc.ProcessDataFrame(new SpeechStartSignal());
            foreach (var doubleData in RandomDataProcessor.CreateFeatVectors(3, 16000, 1000, 39, 10))
                bufferProc.ProcessDataFrame(doubleData);

            bufferProc.ProcessDataFrame(new SpeechEndSignal());
            foreach (var doubleData in RandomDataProcessor.CreateFeatVectors(5, 16000, 2000, 39, 10))
                bufferProc.ProcessDataFrame(doubleData);

            bufferProc.ProcessDataFrame(new DataEndSignal(123));

            return bufferProc;
        }

        //[TestMethod]
        //public void Scorer_ThreadedScorerDeallocation()
        //{
        //    var props = new HashMap<String, Object>();
        //    var dummyFrontEnd = CreateDummyFrontEnd();

        //    props.Put(SimpleAcousticScorer.FeatureFrontend, dummyFrontEnd);
        //    props.Put(ThreadedAcousticScorer.PropNumThreads, 4);
        //    props.Put(ConfigurationManagerUtils.GlobalCommonLoglevel, "FINEST");
        //    var scorer = ConfigurationManager.GetInstance<IAcousticScorer>(typeof(ThreadedAcousticScorer), props);

        //    scorer.Allocate();
        //    scorer.StartRecognition();

        //    var dummyTokens = Arrays.AsList(_testToken);

        //    // score around a little
        //    scorer.CalculateScores(dummyTokens);

        //    scorer.StopRecognition();
        //    scorer.Deallocate();

        //    Thread.Sleep(1000);

        //    // ensure that all scoring threads have died
        //}
    }
}
