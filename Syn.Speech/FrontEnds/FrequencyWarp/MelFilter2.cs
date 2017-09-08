using System;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.FrequencyWarp
{
    /// <summary>
    /// Defines a triangular mel-filter.
    /// The {@link MelFrequencyFilterBank2} creates mel-filters and filters spectrum
    /// data using the method {@link #filterOutput(double[]) filterOutput}.
    ///
    /// A mel-filter is a triangular shaped bandpass filter. When a mel-filter is
    /// constructed, the parameters <code>leftEdge</code>, <code>rightEdge</code>,
    /// <code>centerFreq</code>, <code>initialFreq</code>, and
    /// <code>deltaFreq</code> are given to the {@link MelFilter2 Constructor}. The
    /// first three arguments to the constructor, i.e. <code>leftEdge</code>,
    /// <code>rightEdge</code>, and <code>centerFreq</code>, specify the filter's
    /// slopes. The total area under the filter is 1. The filter is shaped as a
    /// triangle. Knowing the distance between the center frequency and each of the
    /// edges, it is easy to compute the slopes of the two sides in the triangle -
    /// the third side being the frequency axis. The last two arguments,
    /// <code>initialFreq</code> and <code>deltaFreq</code>, identify the first
    /// frequency bin that falls inside this filter and the spacing between
    /// successive frequency bins. All frequencies here are considered in a linear
    /// scale.
    /// 
    /// <see cref="MelFrequencyFilterBank2"/>
    /// </summary>
    public class MelFilter2
    {

        private readonly int _offset;
        private readonly double[] _weights;

        public MelFilter2(double center, double delta, double[] melPoints)
        {
            var lastIndex = 0;
            var firstIndex = melPoints.Length;
            var left = center - delta;
            var right = center + delta;
            var heights = new double[melPoints.Length];

            for (var i = 0; i < heights.Length; ++i)
            {
                if (left < melPoints[i] && melPoints[i] <= center)
                {
                    heights[i] = (melPoints[i] - left) / (center - left);
                    firstIndex = Math.Min(i, firstIndex);
                    lastIndex = i;
                }

                if (center < melPoints[i] && melPoints[i] < right)
                {
                    heights[i] = (right - melPoints[i]) / (right - center);
                    lastIndex = i;
                }
            }

            _offset = firstIndex;
            _weights = Java.CopyOfRange(heights, firstIndex, lastIndex + 1);
        }

        public double Apply(double[] powerSpectrum)
        {
            double result = 0;
            for (var i = 0; i < _weights.Length; ++i)
                result += _weights[i] * powerSpectrum[_offset + i];

            return result;
        }
    }
}
