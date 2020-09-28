using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.FrequencyWarp
{
    ///  <summary>
    ///  /**
    ///  Defines a triangular mel-filter. The {@link edu.cmu.sphinx.frontend.frequencywarp.MelFrequencyFilterBank} creates
    ///  mel-filters and filters spectrum data using the method {@link #filterOutput(double[]) filterOutput}.
    ///  <p/>
    ///  A mel-filter is a triangular shaped bandpass filter.  When a mel-filter is constructed, the parameters
    ///  <code>leftEdge</code>, <code>rightEdge</code>, <code>centerFreq</code>, <code>initialFreq</code>, and
    ///  <code>deltaFreq</code> are given to the {@link MelFilter Constructor}. The first three arguments to the constructor,
    ///  i .e. <code>leftEdge</code>, <code>rightEdge</code>, and <code>centerFreq</code>, specify the filter's slopes. The
    ///  total area under the filter is 1. The filter is shaped as a triangle. Knowing the distance between the center
    ///  frequency and each of the edges, it is easy to compute the slopes of the two sides in the triangle - the third side
    ///  being the frequency axis. The last two arguments, <code>initialFreq</code> and <code>deltaFreq</code>, identify the
    ///  first frequency bin that falls inside this filter and the spacing between successive frequency bins. All frequencies
    ///  here are considered in a linear scale.
    ///  <p/>
    ///  </summary>
    public class MelFilter
    {
        private readonly double[] _weight;
        private readonly int _initialFreqIndex;

        /// <summary>
        /// Constructs a filter from the parameters.
        /// <p/>
        /// In the current implementation, the filter is a bandpass filter with a triangular shape.  We're given the left and
        /// right edges and the center frequency, so we can determine the right and left slopes, which could be not only
        /// asymmetric but completely different. We're also given the initial frequency, which may or may not coincide with
        /// the left edge, and the frequency step.
        /// </summary>
        /// <param name="leftEdge">The filter's lowest passing frequency.</param>
        /// <param name="centerFreq">The filter's center frequency.</param>
        /// <param name="rightEdge">The filter's highest passing frequency.</param>
        /// <param name="initialFreq">The first frequency bin in the pass band.</param>
        /// <param name="deltaFreq">The step in the frequency axis between frequency bins.</param>
        /// <exception cref="System.ArgumentException">
        /// deltaFreq has zero value
        /// or
        /// Filter boundaries too close
        /// or
        /// Number of elements in mel
        ///                         +  is zero.
        /// </exception>
        public MelFilter(double leftEdge,
                         double centerFreq,
                         double rightEdge,
                         double initialFreq,
                         double deltaFreq)
        {
            double filterHeight;
            double leftSlope;
            double rightSlope;
            double currentFreq;
            int indexFilterWeight;
            int numberElementsWeightField;

            if (deltaFreq == 0)
            {
                throw new ArgumentException("deltaFreq has zero value");
            }
            /**
            /// Check if the left and right boundaries of the filter are
            /// too close.
             */
            if ((Math.Round(rightEdge - leftEdge) == 0)
                    || (Math.Round(centerFreq - leftEdge) == 0)
                    || (Math.Round(rightEdge - centerFreq) == 0))
            {
                throw new ArgumentException("Filter boundaries too close");
            }
            /**
            /// Let's compute the number of elements we need in the
            /// <code>weight</code> field by computing how many frequency
            /// bins we can fit in the current frequency range.
             */
            numberElementsWeightField =
                    (int)Math.Round((rightEdge - leftEdge) / deltaFreq + 1);
            /**
            /// Initialize the <code>weight</code> field.
             */
            if (numberElementsWeightField == 0)
            {
                throw new ArgumentException("Number of elements in mel"
                        + " is zero.");
            }
            _weight = new double[numberElementsWeightField];

            /**
            /// Let's make the filter area equal to 1.
             */
            filterHeight = 2.0f / (rightEdge - leftEdge);

            /**
            /// Now let's compute the slopes based on the height.
             */
            leftSlope = filterHeight / (centerFreq - leftEdge);
            rightSlope = filterHeight / (centerFreq - rightEdge);

            /**
            /// Now let's compute the weight for each frequency bin.  We
            /// initialize and update two variables in the <code>for</code>
            /// line.
             */
            for (currentFreq = initialFreq, indexFilterWeight = 0;
                 currentFreq <= rightEdge;
                 currentFreq += deltaFreq, indexFilterWeight++)
            {
                /**
                /// A straight line that contains point <b>(x0, y0)</b> and
                /// has slope <b>m</b> is defined by:
                 *
                /// <b>y = y0 + m/// (x - x0)</b>
                 *
                /// This is used for both "sides" of the triangular filter
                /// below.
                 */
                if (currentFreq < centerFreq)
                {
                    _weight[indexFilterWeight] = leftSlope
                           * (currentFreq - leftEdge);
                }
                else
                {
                    _weight[indexFilterWeight] = filterHeight + rightSlope
                           * (currentFreq - centerFreq);
                }
            }
            /**
            /// Initializing frequency related fields.
             */
            _initialFreqIndex = (int)Math.Round
                    (initialFreq / deltaFreq);
        }


        /// <summary>
        /// Compute the output of a filter. We're given a power spectrum, to which we apply the appropriate weights.
        /// </summary>
        /// <param name="spectrum">The input power spectrum to be filtered.</param>
        /// <returns>The filtered value, in fact a weighted average of power in the frequency range of the filter pass band.</returns>
        public double FilterOutput(double[] spectrum)
        {
            double output = 0.0f;
            int indexSpectrum;

            for (var i = 0; i < _weight.Length; i++)
            {
                indexSpectrum = _initialFreqIndex + i;
                if (indexSpectrum < spectrum.Length)
                {
                    output += spectrum[indexSpectrum] * _weight[i];
                }
            }
            return output;
        }
    }
}
