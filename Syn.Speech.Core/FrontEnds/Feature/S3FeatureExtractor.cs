//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{
    /// <summary>
    /// Computes the delta and double delta of input cepstrum (or plp or ...). The delta is the first order derivative and
    /// the double delta (a.k.a. delta delta) is the second order derivative of the original cepstrum. They help model the
    /// speech signal dynamics. The output data is a {@link FloatData} object with a float array of size three times the
    /// original cepstrum.
    /// <p/>
    /// <p/>
    /// The format of the outputted feature is:
    /// <p/>
    /// 12 cepstra (c[1] through c[12]) <br>followed by delta cepstra (delta c[1] through delta c[12]) <br>followed by c[0],
    /// delta c[0] <br>followed by delta delta c[0] through delta delta c[12] </p>
    /// </summary>
    public class S3FeatureExtractor : AbstractFeatureExtractor
    {
        /// <summary>
        /// Computes the next feature. Advances the pointers as well.
        /// </summary>
        /// <returns>
        /// The feature Data computed.
        /// </returns>
        public override IData ComputeNextFeature()
        {

            var jp1 = (CurrentPosition - 1 + CepstraBufferSize) % CepstraBufferSize;
            var jp2 = (CurrentPosition - 2 + CepstraBufferSize) % CepstraBufferSize;
            var jp3 = (CurrentPosition - 3 + CepstraBufferSize) % CepstraBufferSize;
            var jf1 = (CurrentPosition + 1) % CepstraBufferSize;
            var jf2 = (CurrentPosition + 2) % CepstraBufferSize;
            var jf3 = (CurrentPosition + 3) % CepstraBufferSize;

            var currentCepstrum = CepstraBuffer[CurrentPosition];
            var mfc3f = CepstraBuffer[jf3].Values;
            var mfc2f = CepstraBuffer[jf2].Values;
            var mfc1f = CepstraBuffer[jf1].Values;
            var current = currentCepstrum.Values;
            var mfc1p = CepstraBuffer[jp1].Values;
            var mfc2p = CepstraBuffer[jp2].Values;
            var mfc3p = CepstraBuffer[jp3].Values;
            var feature = new float[current.Length * 3];

            CurrentPosition = (CurrentPosition + 1) % CepstraBufferSize;

            // CEP; skip C[0]
            var j = 0;
            for (var k = 1; k < current.Length; k++)
            {
                feature[j++] = (float)current[k];
            }

            // DCEP: mfc[2] - mfc[-2], skip DC[0]
            for (var k = 1; k < mfc2f.Length; k++)
            {
                feature[j++] = (float)(mfc2f[k] - mfc2p[k]);
            }

            // POW: C0, DC0
            feature[j++] = (float)current[0];
            feature[j++] = (float)(mfc2f[0] - mfc2p[0]);

            // D2CEP: (mfc[3] - mfc[-1]) - (mfc[1] - mfc[-3])
            for (var k = 0; k < mfc3f.Length; k++)
            {
                feature[j++] = (float)
                        ((mfc3f[k] - mfc1p[k]) - (mfc1f[k] - mfc3p[k]));
            }

            return (new FloatData(feature,
                    currentCepstrum.SampleRate,
                    currentCepstrum.FirstSampleNumber));
        }
    }

}
