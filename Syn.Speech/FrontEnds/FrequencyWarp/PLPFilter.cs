using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.FrequencyWarp
{
    /**
     * Defines a filter used by the {@link PLPFrequencyFilterBank} class. The filter is defined by a function in the {@link
     * #PLPFilter Constructor}. A set of equally spaced frequencies in a linear scale is passed to the constructor, which
     * returns the weights for each of the frequency bins, such that the filter has the shape defined by this piecewise
     * function in the bark scale.
     *
     * @author <a href="mailto:rsingh@cs.cmu.edu">rsingh</a>
     * @version 1.0
     * @see PLPFrequencyFilterBank
     */
    public class PLPFilter
    {

        private readonly double[] _filterCoefficients;
        private readonly int _numDftPoints;

        /// <summary>
        /// The center frequency of the filter in Hertz. 
        /// </summary>
        public double CenterFreqInHz;

        /// <summary>
        /// The center frequency of the filter in Bark.
        /// </summary>
        public double CenterFreqInBark;

        /// <summary>
        /// Initializes a new instance of the <see cref="PLPFilter"/> class.
        /// <para> Defines a filter according to the following equation, defined piecewise (all frequencies in the equation are Bark
        /// frequencies):</para>
        /// Filter(f) = 0 if f < -2.5 <br>
        ///           = 10^(-(f+0.5)) if -2.5 <= f <= -0.5 <br>
        ///           = 1  if -0.5 <= f <= 0.5 <br>
        ///           = 10^(2.5(f-0.5)) if 0.5 <= f <= 1.3 <br>
        ///           = 0 if f > 1.3 <br>
        /// The current implementation assumes that the calling routine passes in an array of frequencies, one for each of
        /// the DFT points in the spectrum of the frame of speech to be filtered. This is used in conjunction with a
        /// specified center frequency to determine the filter.
        ///
        /// </summary>
        /// <param name="dftFrequenciesInHz">A double array containing the frequencies in Hertz corresponding to each of the DFT points in the spectrum of the signal to be filtered.</param>
        /// <param name="centerFreqInHz">The filter's center frequency.</param>
        /// <exception cref="System.ArgumentException">Center frequency for PLP filter out of range</exception>
        public PLPFilter(double[] dftFrequenciesInHz, double centerFreqInHz)
        {

            var bark = new FrequencyWarper();

            _numDftPoints = dftFrequenciesInHz.Length;
            CenterFreqInHz = centerFreqInHz;
            CenterFreqInBark = bark.HertzToBark(centerFreqInHz);

            if (centerFreqInHz < dftFrequenciesInHz[0] ||
                    centerFreqInHz > dftFrequenciesInHz[_numDftPoints - 1])
            {
                throw new ArgumentException("Center frequency for PLP filter out of range");
            }

            _filterCoefficients = new double[_numDftPoints];

            for (var i = 0; i < _numDftPoints; i++)
            {
                double barkf = bark.HertzToBark(dftFrequenciesInHz[i]) - CenterFreqInBark;
                if (barkf < -2.5)
                    _filterCoefficients[i] = 0.0;
                else if (barkf <= -0.5)
                    _filterCoefficients[i] = Math.Pow(10.0, barkf + 0.5);
                else if (barkf <= 0.5)
                    _filterCoefficients[i] = 1.0;
                else if (barkf <= 1.3)
                    _filterCoefficients[i] = Math.Pow(10.0, -2.5 * (barkf - 0.5));
                else
                    _filterCoefficients[i] = 0.0;
            }
        }


        /// <summary>
        /// Compute the PLP spectrum at the center frequency of this filter for a given power spectrum.
        /// </summary>
        /// <param name="spectrum">The input power spectrum to be filtered.</param>
        /// <returns>The PLP spectrum value</returns>
        /// <exception cref="System.ArgumentException">Mismatch in no. of DFT points  + spectrum.Length +
        ///                                  in spectrum and in filter  + _numDftPoints</exception>
        public double FilterOutput(double[] spectrum)
        {

            if (spectrum.Length != _numDftPoints)
            {
                throw new ArgumentException
                        ("Mismatch in no. of DFT points " + spectrum.Length +
                                " in spectrum and in filter " + _numDftPoints);
            }

            var output = 0.0;
            for (var i = 0; i < _numDftPoints; i++)
            {
                output += spectrum[i] * _filterCoefficients[i];
            }
            return output;
        }
    }
}
