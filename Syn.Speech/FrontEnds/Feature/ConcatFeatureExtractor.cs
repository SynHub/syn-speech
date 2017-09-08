//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{
    /// <summary>
    /// This component concatenate the cepstrum from the sequence of frames according to the window size.
    /// It's not supposed to give high accuracy alone, but combined with LDA transform it can give the same
    /// or even better results than conventional delta and delta-delta coefficients. The idea is that
    /// delta-delta computation is also a matrix multiplication thus using automatically generated
    /// with LDA/MLLT matrix we can gain better results.
    /// The model for this feature extractor should be trained with SphinxTrain with 1s_c feature type and
    /// with cepwin option enabled. Don't forget to set the window size accordingly.
    /// </summary>
    public class ConcatFeatureExtractor : AbstractFeatureExtractor
    {

        public ConcatFeatureExtractor(int window)
            : base(window)
        {

        }

        public ConcatFeatureExtractor()
        {
        }

        /// <summary>
        /// Computes the next feature. Advances the pointers as well.
        /// </summary>
        /// <returns>
        /// The feature Data computed.
        /// </returns>
        public override IData ComputeNextFeature()
        {
            DoubleData currentCepstrum = CepstraBuffer[CurrentPosition];
            float[] feature = new float[(Window * 2 + 1) * currentCepstrum.Values.Length];
            int j = 0;
            for (int k = -Window; k <= Window; k++)
            {
                int position = (CurrentPosition + k + CepstraBufferSize) % CepstraBufferSize;
                double[] buffer = CepstraBuffer[position].Values;
                foreach (double val in buffer)
                {
                    feature[j++] = (float)val;
                }
            }
            CurrentPosition = (CurrentPosition + 1) % CepstraBufferSize;

            return (new FloatData(feature,
                    currentCepstrum.SampleRate,
                    currentCepstrum.FirstSampleNumber));
        }
    }

}
