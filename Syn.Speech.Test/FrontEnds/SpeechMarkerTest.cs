using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.FrontEnds;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.FrontEnds
{
    [TestClass]
    public class SpeechMarkerTest : RandomDataProcessor
    {
        public BaseDataProcessor CreateDataFilter(bool mergeSpeechSegments)
        {
            try
            {
                var speechMarker = ConfigurationManager.GetInstance<SpeechMarker>();
                speechMarker.Initialize();
                return speechMarker;
            }
            catch (PropertyException e)
            {
                e.PrintStackTrace();
            }

            return null;
        }

        [TestMethod]
        public void SpeechMarker_EndWithoutSilence()
        {
            const int sampleRate = 1000;
            Input.Add(new DataStartSignal(sampleRate));
            Input.AddRange(createClassifiedSpeech(sampleRate, 2, true));
            Input.Add(new DataEndSignal(-2));

            var results = CollectOutput(CreateDataFilter(false));

            Assert.IsTrue(results.Count == 104);//TODO: Fails because an extra code is added to SpeechMarker
            Assert.IsTrue(results[0] is DataStartSignal);
            Assert.IsTrue(results[1] is SpeechStartSignal);
            Assert.IsTrue(results[102] is SpeechEndSignal);
            Assert.IsTrue(results[103] is DataEndSignal);
        }

        private List<SpeechClassifiedData> createClassifiedSpeech(int sampleRate, double lengthSec, bool isSpeech)
        {
            List<SpeechClassifiedData> datas = new List<SpeechClassifiedData>();
            List<DoubleData> featVecs = CreateFeatVectors(1, sampleRate, 0, 10, 10);

            foreach (DoubleData featVec in featVecs)
                datas.Add(new SpeechClassifiedData(featVec, isSpeech));

            return datas;
        }

    }
}
