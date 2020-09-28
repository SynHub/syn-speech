using System;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.FrequencyWarp
{
    /// <summary>
    /// Filters an input power spectrum through a bank of number of mel-filters. The output is an array of filtered values,
    /// typically called mel-spectrum, each corresponding to the result of filtering the input spectrum through an individual
    /// filter. Therefore, the length of the output array is equal to the number of filters created.
    /// 
    /// The triangular mel-filters in the filter bank are placed in the frequency axis so that each filter's center frequency
    /// follows the mel scale, in such a way that the filter bank mimics the critical band, which represents different
    /// perceptual effect at different frequency bands. Additionally, the edges are placed so that they coincide with the
    /// center frequencies in adjacent filters. Pictorially, the filter bank looks like:
    ///
    /// <img src="doc-files/melfilterbank.jpg"> <br> <center><b>Figure 1: A Mel-filter bank. </b> </center>
    /// <p/>
    /// As you might notice in the above figure, the distance at the base from the center to the left edge is different from
    /// the center to the right edge. Since the center frequencies follow the mel-frequency scale, which is a non-linear
    /// scale that models the non-linear human hearing behavior, the mel filter bank corresponds to a warping of the
    /// frequency axis. As can be inferred from the figure, filtering with the mel scale emphasizes the lower frequencies. A
    /// common model for the relation between frequencies in mel and linear scales is as follows:
    /// <p/>
    /// <code>melFrequency = 2595/// log(1 + linearFrequency/700)</code>
    /// <p/>
    /// The constants that define the filterbank are the number of filters, the minimum frequency, and the maximum frequency.
    /// The minimum and maximum frequencies determine the frequency range spanned by the filterbank. These frequencies depend
    /// on the channel and the sampling frequency that you are using. For telephone speech, since the telephone channel
    /// corresponds to a bandpass filter with cutoff frequencies of around 300Hz and 3700Hz, using limits wider than these
    /// would waste bandwidth. For clean speech, the minimum frequency should be higher than about 100Hz, since there is no
    /// speech information below it. Furthermore, by setting the minimum frequency above 50/60Hz, we get rid of the hum
    /// resulting from the AC power, if present.
    /// <p/>
    /// The maximum frequency has to be lower than the Nyquist frequency, that is, half the sampling rate. Furthermore, there
    /// is not much information above 6800Hz that can be used for improving separation between models. Particularly for very
    /// noisy channels, maximum frequency of around 5000Hz may help cut off the noise.
    /// <p/>
    /// Typical values for the constants defining the filter bank are: <table width="80%" border="1"> <tr> <td><b>Sample rate
    /// (Hz) </b></td> <td><b>16000 </b></td> <td><b>11025 </b></td> <td><b>8000 </b></td> </tr> <tr> <td>{@link
    /// #PROP_NUMBER_FILTERS numberFilters}</td> <td>40</td> <td>36</td> <td>31</td> </tr> <tr> <td>{@link #PROP_MIN_FREQ
    /// minimumFrequency}(Hz)</td> <td>130</td> <td>130</td> <td>200</td> </tr> <tr> <td>{@link #PROP_MAX_FREQ
    /// maximumFrequency}(Hz)</td> <td>6800</td> <td>5400</td> <td>3500</td> </tr> </table>
    /// <p/>
    /// Davis and Mermelstein showed that Mel-frequency cepstral coefficients present robust characteristics that are good
    /// for speech recognition. For details, see Davis and Mermelstein, <i>Comparison of Parametric Representations for
    /// Monosyllable Word Recognition in Continuously Spoken Sentences, IEEE Transactions on Acoustic, Speech and Signal
    /// Processing, 1980 </i>.
    /// </summary>
    public class MelFrequencyFilterBank: BaseDataProcessor
    {
        /// <summary>
        ///The property for the number of filters in the filterbank.
        /// </summary>
        [S4Integer(DefaultValue = 40)]
        public static string PropNumberFilters = "numberFilters";

        /// <summary>
        /// The property for the minimum frequency covered by the filterbank.
        /// </summary>
        [S4Double(DefaultValue = 130.0)]
        public static string PropMinFreq = "minimumFrequency";

        /// <summary>
        /// The property for the maximum frequency covered by the filterbank.
        /// </summary>
        [S4Double(DefaultValue = 6800.0)]
        public static string PropMaxFreq = "maximumFrequency";

        // ----------------------------------
        // Configuration data
        // ----------------------------------
        private int _sampleRate;
        private int _numberFftPoints;
        private int _numberFilters;
        private double _minFreq;
        private double _maxFreq;
        private MelFilter[] _filter;


        public MelFrequencyFilterBank(double minFreq, double maxFreq, int numberFilters) 
        {
            _minFreq = minFreq;
            _maxFreq = maxFreq;
            _numberFilters = numberFilters;
        }

        public MelFrequencyFilterBank() {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            _minFreq = ps.GetDouble(PropMinFreq);
            _maxFreq = ps.GetDouble(PropMaxFreq);
            _numberFilters = ps.GetInt(PropNumberFilters);
        }

        public override void Initialize() 
        {
            base.Initialize();
        }

        /// <summary>
        /// Compute mel frequency from linear frequency.
        /// <para>Since we don't have <code>log10()</code>, we have to compute it using natural log: <b>log10(x) = ln(x) / ln(10)</b></para>
        /// </summary>
        /// <param name="inputFreq">The input frequency in linear scale.</param>
        /// <returns>The frequency in a mel scale.</returns>
        private double LinToMelFreq(double inputFreq) {
            return (2595.0* (Math.Log(1.0 + inputFreq / 700.0) / Math.Log(10.0)));
        }

        /// <summary>
        /// Compute linear frequency from mel frequency.
        /// </summary>
        /// <param name="inputFreq">The input frequency in mel scale.</param>
        /// <returns>The frequency in a linear scale.</returns>
        private double MelToLinFreq(double inputFreq) {
            return (700.0* (Math.Pow(10.0, (inputFreq / 2595.0)) - 1.0));
        }


        /// <summary>
        /// Sets the given frequency to the nearest frequency bin from the FFT. The FFT can be thought of as a sampling of
        /// the actual spectrum of a signal. We use this function to find the sampling point of the spectrum that is closest
        /// to the given frequency.
        /// </summary>
        /// <param name="inFreq">The input frequency.</param>
        /// <param name="stepFreq">The distance between frequency bins.</param>
        /// <returns>The closest frequency bin.</returns>
        /// <exception cref="System.ArgumentException">stepFreq is zero</exception>
        private double SetToNearestFrequencyBin(double inFreq, double stepFreq)
        {
            if (stepFreq == 0) {
                throw new ArgumentException("stepFreq is zero");
            }
            return stepFreq* Math.Round(inFreq / stepFreq);
        }

        /// <summary>
        /// Build a mel filterbank with the parameters given. Each filter will be shaped as a triangle. The triangles overlap
        /// so that they cover the whole frequency range requested. The edges of a given triangle will be by default at the
        /// center of the neighboring triangles.
        /// </summary>
        /// <param name="numberFftPoints">Number of points in the power spectrum.</param>
        /// <param name="numberFilters">The number of filters in the filterbank.</param>
        /// <param name="minFreq">The lowest frequency in the range of interest.</param>
        /// <param name="maxFreq">The highest frequency in the range of interest.</param>
        /// <exception cref="System.ArgumentException">
        /// Number of FFT points is zero
        /// or
        /// Number of filters illegal: 
        ///                         + numberFilters
        /// </exception>
        private void BuildFilterbank(int numberFftPoints, int numberFilters,
                                     double minFreq, double maxFreq)
        {
            double minFreqMel;
            double maxFreqMel;
            double deltaFreqMel;
            var leftEdge = new double[numberFilters];
            var centerFreq = new double[numberFilters];
            var rightEdge = new double[numberFilters];
            double nextEdgeMel;
            double nextEdge;
            double initialFreqBin;
            double deltaFreq;
            _filter = new MelFilter[numberFilters];
            /**
            /// In fact, the ratio should be between <code>sampleRate /
            /// 2</code>
            /// and <code>numberFftPoints / 2</code> since the number of points in
            /// the power spectrum is half of the number of FFT points - the other
            /// half would be symmetrical for a real sequence -, and these points
            /// cover up to the Nyquist frequency, which is half of the sampling
            /// rate. The two "divide by 2" get canceled out.
             */
            if (numberFftPoints == 0) {
                throw new ArgumentException("Number of FFT points is zero");
            }
            deltaFreq = (double) _sampleRate / numberFftPoints;
            /**
            /// Initialize edges and center freq. These variables will be updated so
            /// that the center frequency of a filter is the right edge of the
            /// filter to its left, and the left edge of the filter to its right.
             */
            if (numberFilters < 1) {
                throw new ArgumentException("Number of filters illegal: "
                        + numberFilters);
            }
            minFreqMel = LinToMelFreq(minFreq);
            maxFreqMel = LinToMelFreq(maxFreq);
            deltaFreqMel = (maxFreqMel - minFreqMel) / (numberFilters + 1);
            leftEdge[0] = SetToNearestFrequencyBin(minFreq, deltaFreq);
            nextEdgeMel = minFreqMel;
            for (var i = 0; i < numberFilters; i++) {
                nextEdgeMel += deltaFreqMel;
                nextEdge = MelToLinFreq(nextEdgeMel);
                centerFreq[i] = SetToNearestFrequencyBin(nextEdge, deltaFreq);
                if (i > 0) {
                    rightEdge[i - 1] = centerFreq[i];
                }
                if (i < numberFilters - 1) {
                    leftEdge[i + 1] = centerFreq[i];
                }
            }
            nextEdgeMel = nextEdgeMel + deltaFreqMel;
            nextEdge = MelToLinFreq(nextEdgeMel);
            rightEdge[numberFilters - 1] = SetToNearestFrequencyBin(nextEdge,
                    deltaFreq);
            for (var i = 0; i < numberFilters; i++) {
                initialFreqBin = SetToNearestFrequencyBin(leftEdge[i], deltaFreq);
                if (initialFreqBin < leftEdge[i]) {
                    initialFreqBin += deltaFreq;
                }
                //System.out.format("%d %f %f\n", i, leftEdge[i], rightEdge[i]);
                _filter[i] = new MelFilter(leftEdge[i], centerFreq[i],
                        rightEdge[i], initialFreqBin, deltaFreq);
            }
        }


        /// <summary>
        /// Process data, creating the power spectrum from an input audio frame.
        /// </summary>
        /// <param name="input">input power spectrum</param>
        /// <returns>power spectrum</returns>
        private DoubleData Process(DoubleData input)
        {
            var _in = input.Values;

            if (_filter == null || _sampleRate != input.SampleRate) 
            {
                _numberFftPoints = (_in.Length - 1) << 1;
                _sampleRate = input.SampleRate;
                BuildFilterbank(_numberFftPoints, _numberFilters, _minFreq, _maxFreq);
            } 
            else if (_in.Length != ((_numberFftPoints >> 1) + 1)) 
            {
                throw new ArgumentException(
                        "Window size is incorrect: in.length == " + _in.Length
                                + ", numberFftPoints == "
                                + ((_numberFftPoints >> 1) + 1));
            }
            var output = new double[_numberFilters];
            
            /// Filter input power spectrum
            for (var i = 0; i < _numberFilters; i++) 
            {
                output[i] = _filter[i].FilterOutput(_in);
            }
            var outputMelSpectrum = new DoubleData(output,
                    _sampleRate, input.FirstSampleNumber);
            return outputMelSpectrum;
        }

        /// <summary>
        /// Reads the next Data object, which is the power spectrum of an audio input frame. 
        /// Signals are returned unmodified.
        /// </summary>
        /// <returns>the next available Data or Signal object, or returns null if no Data is available</returns>
        public override IData GetData()
        {
            var input = Predecessor.GetData();
            if (input != null) 
            {
                if (input is DoubleData) 
                {
                    input = Process((DoubleData) input);
                }
            }
            return input;
        }
    }
}
