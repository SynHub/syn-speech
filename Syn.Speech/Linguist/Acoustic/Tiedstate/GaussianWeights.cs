using System;
using System.Diagnostics;
using Syn.Speech.Logging;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    /// <summary>
    /// Structure to store weights for all gaussians in AM. 
    ///  Supposed to provide faster access in case of large models
    /// </summary>
    public class GaussianWeights
    {

        private readonly float[,] _weights;

        public GaussianWeights(String name, int numStates, int gauPerState, int numStreams)
        {
            StatesNum = numStates;
            GauPerState = gauPerState;
            StreamsNum = numStreams;
            Name = name;
            _weights = new float[gauPerState, numStates * numStreams];
        }

        public void Put(int stateId, int streamId, float[] gauWeights)
        {
            Debug.Assert(gauWeights.Length == GauPerState);
            for (var i = 0; i < GauPerState; i++)
                _weights[i, stateId * StreamsNum + streamId] = gauWeights[i];
        }

        public float Get(int stateId, int streamId, int gaussianId)
        {
            return _weights[gaussianId, stateId * StreamsNum + streamId];
        }

        public int StatesNum { get; private set; }

        public int GauPerState { get; private set; }

        public int StreamsNum { get; private set; }

        public string Name { get; private set; }

        public void LogInfo()
        {
            this.LogInfo("Gaussian weights: " + Name + ". Entries: " + StatesNum * StreamsNum);
        }

        public Pool<float[]> ConvertToPool()
        {
            return null;
        }
    }

}
