//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{
    /*
     * Copyright 2013 Carnegie Mellon University.  
     * All Rights Reserved.  Use is subject to license terms.
     * 
     * See the file "license.terms" for information on usage and
     * redistribution of this file, and for a DISCLAIMER OF ALL 
     * WARRANTIES.
     *
     */
    public class KaldiDeltasFeatureExtractor : AbstractFeatureExtractor
    {

        public KaldiDeltasFeatureExtractor(int window): base(window)
        {

        }

        public KaldiDeltasFeatureExtractor()
        {
        }


        public override IData ComputeNextFeature()
        {
            var jp1 = (CurrentPosition - 1 + CepstraBufferSize) % CepstraBufferSize;
            var jp2 = (CurrentPosition - 2 + CepstraBufferSize) % CepstraBufferSize;
            var jp3 = (CurrentPosition - 3 + CepstraBufferSize) % CepstraBufferSize;
            var jp4 = (CurrentPosition - 4 + CepstraBufferSize) % CepstraBufferSize;
            var jf1 = (CurrentPosition + 1) % CepstraBufferSize;
            var jf2 = (CurrentPosition + 2) % CepstraBufferSize;
            var jf3 = (CurrentPosition + 3) % CepstraBufferSize;
            var jf4 = (CurrentPosition + 4) % CepstraBufferSize;

            var currentCepstrum = CepstraBuffer[CurrentPosition];
            var mfc4f = CepstraBuffer[jf4].Values;
            var mfc3f = CepstraBuffer[jf3].Values;
            var mfc2f = CepstraBuffer[jf2].Values;
            var mfc1f = CepstraBuffer[jf1].Values;
            var current = currentCepstrum.Values;
            var mfc1p = CepstraBuffer[jp1].Values;
            var mfc2p = CepstraBuffer[jp2].Values;
            var mfc3p = CepstraBuffer[jp3].Values;
            var mfc4p = CepstraBuffer[jp4].Values;
            var feature = new float[current.Length * 3];

            CurrentPosition = (CurrentPosition + 1) % CepstraBufferSize;

            var j = 0;
            foreach (var val in current)
            {
                feature[j++] = (float)val;
            }
            for (var k = 0; k < mfc2f.Length; k++)
            {
                feature[j++] = (float)(2 * mfc2f[k] + mfc1f[k] - mfc1p[k] - 2 * mfc2p[k]) / 10.0f;
            }

            for (var k = 0; k < mfc3f.Length; k++)
            {
                feature[j++] = (float)((4 * mfc4f[k] + 4 * mfc3f[k] + mfc2f[k] - 4 * mfc1f[k]) - 10 * current[k] +
                    (4 * mfc4p[k] + 4 * mfc3p[k] + mfc2p[k] - 4 * mfc1p[k])) / 100.0f;
            }
            return (new FloatData(feature,
                    currentCepstrum.SampleRate,
                    currentCepstrum.FirstSampleNumber));
        }
    }
}
