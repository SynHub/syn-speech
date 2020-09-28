using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate.Tiedmixture
{
    /**
     * Class to keep scores of mixture components for certain frame.
     * Is use useful in case of fast match to avoid scoring gaussians twice
     */
    public class MixtureComponentSetScores
    {
        private readonly float[][] _scores; //scores[featureStreamIdx][gaussianIndex]
        private readonly int[][] _ids;       //id[featureStreamIdx][gaussianIndex]

        public MixtureComponentSetScores(int numStreams, int gauNum, long frameStartSample)
        {
            _scores = Java.CreateArray<float[][]>(numStreams, gauNum); // new float[numStreams][gauNum];
            _ids = Java.CreateArray<int[][]>(numStreams, gauNum); //new int[numStreams][gauNum];
            FrameStartSample = frameStartSample;
        }

        public void SetScore(int featStream, int gauIdx, float score)
        {
            _scores[featStream][gauIdx] = score;
        }

        public void SetGauId(int featStream, int gauIdx, int id)
        {
            _ids[featStream][gauIdx] = id;
        }

        public float GetScore(int featStream, int gauIdx)
        {
            return _scores[featStream][gauIdx];
        }

        public int GetGauId(int featStream, int gauIdx)
        {
            return _ids[featStream][gauIdx];
        }

        public long FrameStartSample { get; private set; }
    }
}
