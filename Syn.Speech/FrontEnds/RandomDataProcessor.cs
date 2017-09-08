using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    /// A DataProcessor implemenation which can be used to setup simple unit-tests for other DataProcessors.
    /// Addtionally some static utility methods which should ease unit-testing of DataProcessors are provided by this class.
    /// @author Holger Brandl
    /// </summary>
    public abstract class RandomDataProcessor : BaseDataProcessor
    {

        public static Random Rnd = new Random(123);

        protected List<IData> Input = new List<IData>();


        public override IData GetData()
        {
            return Input.IsEmpty() ? null : Input.Remove(0);
        }


        public List<IData> CollectOutput(BaseDataProcessor dataProc)
        {
            dataProc.Predecessor = this;

            var output = new List<IData>();

            IData d;
            while ((d = dataProc.GetData()) != null)
            {
                output.Add(d);
            }

            return output;
        }


        public static List<DoubleData> CreateFeatVectors(double lengthSec, int sampleRate, long startSample, int featDim, double shiftMs)
        {
            var numFrames = (int)Math.Ceiling((lengthSec * 1000) / shiftMs);
            var datas = new List<DoubleData>(numFrames);

            var curStartSample = startSample;
            var shiftSamples = Ms2Samples((int)shiftMs, sampleRate);
            for (var i = 0; i < numFrames; i++)
            {
                var values = CreateRandFeatureVector(featDim, null, null);
                datas.Add(new DoubleData(values, sampleRate, curStartSample));

                curStartSample += shiftSamples;
            }

            return datas;
        }


        public static double[] CreateRandFeatureVector(int featDim, double[] mean, double[] variance)
        {
            if (mean == null)
            {
                mean = new double[featDim];
            }

            if (variance == null)
            {
                variance = new double[featDim];
                for (var i = 0; i < variance.Length; i++)
                {
                    variance[i] = 1;
                }
            }

            Debug.Assert(featDim == mean.Length && featDim == variance.Length);

            var updBlock = new double[featDim];

            for (var i = 0; i < updBlock.Length; i++)
            {
                updBlock[i] = mean[i] + variance[i] * Rnd.NextDouble(); // *10 to get better debuggable (sprich: merkbarer) values
            }

            return updBlock;
        }


        public static long Ms2Samples(double ms, int sampleRate)
        {
            return (long)Math.Round(sampleRate * ms / 1000);
        }
    }

}
