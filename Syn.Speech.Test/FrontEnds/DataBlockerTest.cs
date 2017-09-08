using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.FrontEnds;
using Syn.Speech.Helper;

namespace Syn.Speech.Test.FrontEnds
{
    [TestClass]
    public class DataBlockerTest : BaseDataProcessor
    {
        private readonly List<IData> _input;

        public DataBlockerTest()
        {
            _input = new List<IData>();
        }

        [TestMethod]
        public void DataBlocker_LongInput()
        {
            const int sampleRate = 1000;
            _input.Add(new DataStartSignal(sampleRate));
            _input.AddRange(CreateDataInput(1000, 1000, sampleRate, 0));// create one second of data sampled with 1kHz
            _input.Add(new DataEndSignal(0));


            Assert.IsTrue(HasIncreasingOrder(CollectOutput(100),1000));
        }

        [TestMethod]
        public void DataBlocker_UsualInput()
        {
            const int sampleRate = 1000;
            _input.Add(new DataStartSignal(sampleRate));
            _input.AddRange(CreateDataInput(600,120,sampleRate,0));
            _input.Add(new DataEndSignal(0));

            var output = CollectOutput(100);

            Assert.AreEqual(output.Count, 6);
            Assert.AreEqual(201, ((DoubleData)output[2]).FirstSampleNumber);
            Assert.IsTrue(HasIncreasingOrder(output,600));
        }

        [TestMethod]
        public void SkipLastSamples()
        {
            const int sampleRate = 1000;
            _input.Add(new DataStartSignal(sampleRate));
            _input.AddRange(CreateDataInput(500, 500, sampleRate, 0));
            _input.AddRange(CreateDataInput(300, 300, sampleRate, 500));
            _input.Add(new DataEndSignal(0));

            var output = CollectOutput(250);

            Assert.AreEqual(output.Count, 3);
            Assert.AreEqual(501, ((DoubleData)output[2]).FirstSampleNumber);
            Assert.IsTrue(HasIncreasingOrder(output, 750));

        }

        private List<IData> CollectOutput(double blocSizeMs)
        {
            var dataBlocker = new DataBlocker(blocSizeMs) {Predecessor = this};

            var output = new List<IData>();

            while (true)
            {
                var d = dataBlocker.GetData();
                if (d is DoubleData)
                {
                    output.Add(d);
                }
                if (d is DataEndSignal)
                {
                    return output;
                }
            }
        } 




        private static List<DoubleData> CreateDataInput(int numSamples, int blockSize, int sampleRate, int offSet)
        {
            var datas = new List<DoubleData>();

            double counter = 1;
            for (var i = 0; i < numSamples/blockSize; i++)
            {
                var values = new double[blockSize];
                datas.Add(new DoubleData(values, sampleRate, (long) (counter+offSet)));

                for (var j = 0; j < values.Length; j++)
                {
                    values[j] = counter++ + offSet;
                }
            }
            return datas;
        }

        private static bool HasIncreasingOrder(List<IData> output, int lastValue)
        {
            var dataCounter = 0;

            foreach(var data in output )
            {
                if (data is DoubleData)
                {
                    var dd = (DoubleData) data;

                    for (var i = 0; i < dd.Values.Length; i++)
                    {
                        if ((dataCounter + 1) == dd.Values[i])
                        {
                            dataCounter++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            return dataCounter == lastValue;
        }

        public override IData GetData()
        {
            return _input.Remove(0);
        }
    }
}
