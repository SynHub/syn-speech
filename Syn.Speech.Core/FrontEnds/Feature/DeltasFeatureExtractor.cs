//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{
    /// <summary>
    /// Computes the delta and double delta of input cepstrum (or plp or ...). The delta is the first order derivative and
    /// the double delta (a.k.a. delta delta) is the second order derivative of the original cepstrum. They help model the
    /// speech signal dynamics. The output data is a {@link FloatData} object with a float array of size three times the
    /// original cepstrum, formed by the concatenation of cepstra, delta cepstra, and double delta cepstra. The output is the
    /// feature vector used by the decoder. Figure 1 shows the arrangement of the output feature data array:
    /// <p/>
    /// <img src="doc-files/feature.jpg"> <br> <b>Figure 1: Layout of the returned features. </b>
    /// <p/>
    /// Suppose that the original cepstrum has a length of N, the first N elements of the feature are just the original
    /// cepstrum, the second N elements are the delta of the cepstrum, and the last N elements are the double delta of the
    /// cepstrum.
    /// <p/>
    /// Figure 2 below shows pictorially the computation of the delta and double delta of a cepstrum vector, using the last 3
    /// cepstra and the next 3 cepstra. <img src="doc-files/deltas.jpg"> <br> <b>Figure 2: Delta and double delta vector
    /// computation. </b>
    /// <p/>
    /// Referring to Figure 2, the delta is computed by subtracting the cepstrum that is two frames behind of the current
    /// cepstrum from the cepstrum that is two frames ahead of the current cepstrum. The computation of the double delta is
    /// similar. It is computed by subtracting the delta cepstrum one time frame behind from the delta cepstrum one time
    /// frame ahead. Replacing delta cepstra with cepstra, this works out to a formula involving the cepstra that are one and
    /// three behind and after the current cepstrum.
    /// </summary>
    public class DeltasFeatureExtractor : AbstractFeatureExtractor
    {
         /**
         *
        /// @param window
         */
        public DeltasFeatureExtractor( int window )
            : base(window)
        {
            
        }

        public DeltasFeatureExtractor( ) 
        {
        }

        /// <summary>
        /// Computes the next feature. Advances the pointers as well.
        /// </summary>
        /// <returns>
        /// The feature Data computed.
        /// </returns>
        public override IData ComputeNextFeature() {

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
            var feature = new float[current.Length* 3];

            CurrentPosition = (CurrentPosition + 1) % CepstraBufferSize;

            // CEP; copy all the cepstrum data
            var j = 0;
            foreach (var val in current) 
            {
                feature[j++] = (float)val;
            }
            // System.arraycopy(current, 0, feature, 0, j);
            // DCEP: mfc[2] - mfc[-2]
            for (var k = 0; k < mfc2f.Length; k++) 
            {
                feature[j++] = (float) (mfc2f[k] - mfc2p[k]);
            }
            // D2CEP: (mfc[3] - mfc[-1]) - (mfc[1] - mfc[-3])
            for (var k = 0; k < mfc3f.Length; k++) 
            {
                feature[j++] = (float) ((mfc3f[k] - mfc1p[k]) - (mfc1f[k] - mfc3p[k]));
            }
            return (new FloatData(feature,
                    currentCepstrum.SampleRate,
                    currentCepstrum.FirstSampleNumber));
        }

    }
}
