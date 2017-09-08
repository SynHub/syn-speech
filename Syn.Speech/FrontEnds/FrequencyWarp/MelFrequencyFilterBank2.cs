using System;
using System.Diagnostics;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.FrequencyWarp
{
    /// <summary>
    /// Filters an input power spectrum through a bank of number of mel-filters. The
    /// output is an array of filtered values, typically called mel-spectrum, each
    /// corresponding to the result of filtering the input spectrum through an
    /// individual filter. Therefore, the length of the output array is equal to the
    /// number of filters created.
    /// </summary>
    /// <see cref="MelFilter2"/>
    public class MelFrequencyFilterBank2 : BaseDataProcessor
    {
        /// <summary>
        /// The property for the number of filters in the filterbank.
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
        private int _numberFilters;
        private double _minFreq;
        private double _maxFreq;

        private MelFilter2[] _filters;

        public MelFrequencyFilterBank2(double minFreq, double maxFreq, int numberFilters)
        {
            _minFreq = minFreq;
            _maxFreq = maxFreq;
            _numberFilters = numberFilters;
        }

        public MelFrequencyFilterBank2()
        {
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
        /// </summary>
        /// <param name="inputFreq">The input frequency in linear scale.</param>
        /// <returns>The frequency in a mel scale.</returns>
        private double LinearToMel(double inputFreq)
        {
            //return 1127 * Math.log1p(inputFreq / 700);
            //TODO: CHECK SEMANTICS
            return 1127 * Math.Log(1 + (inputFreq / 700));
        }

        /// <summary>
        /// Build a mel filterbank with the parameters given. Each filter will be  shaped as a triangle. The triangles overlap so that they cover the whole
        /// frequency range requested. The edges of a given triangle will be by default at the center of the neighboring triangles.
        /// </summary>
        /// <param name="windowLength">Number of points in the power spectrum.</param>
        /// <param name="numberFilters">Bumber of filters in the filterbank.</param>
        /// <param name="minFreq">The lowest frequency in the range of interest.</param>
        /// <param name="maxFreq">The highest frequency in the range of interest.</param>
        private void BuildFilterbank(int windowLength, int numberFilters, double minFreq,double maxFreq)
        {
            Debug.Assert(windowLength > 0);
            Debug.Assert(numberFilters > 0);
            // Initialize edges and center freq. These variables will be updated so
            // that the center frequency of a filter is the right edge of the
            // filter to its left, and the left edge of the filter to its right.

            var minFreqMel = LinearToMel(minFreq);
            var maxFreqMel = LinearToMel(maxFreq);
            var deltaFreqMel = (maxFreqMel - minFreqMel) / (numberFilters + 1);
            // In fact, the ratio should be between <code>sampleRate /
            // 2</code> and <code>numberFftPoints / 2</code> since the number of
            // points in the power spectrum is half of the number of FFT points -
            // the other half would be symmetrical for a real sequence -, and
            // these points cover up to the Nyquist frequency, which is half of
            // the sampling rate. The two "divide by 2" get canceled out.
            var deltaFreq = (double)_sampleRate / windowLength;
            var melPoints = new double[windowLength / 2];
            _filters = new MelFilter2[numberFilters];

            for (var i = 0; i < windowLength / 2; ++i)
                melPoints[i] = LinearToMel(i * deltaFreq);

            for (var i = 0; i < numberFilters; i++)
            {
                var centerMel = minFreqMel + (i + 1) * deltaFreqMel;
                _filters[i] = new MelFilter2(centerMel, deltaFreqMel, melPoints);
            }
        }

        /// <summary>
        /// Process data, creating the power spectrum from an input audio frame.
        /// </summary>
        /// <param name="input">The input power spectrum.</param>
        /// <returns>power spectrum</returns>
        /// <exception cref="System.ArgumentException">Window size is incorrect: in.length == 
        ///                         + values.Length
        ///                         + , numberFftPoints == 
        ///                         + ((windowLength >> 1) + 1)</exception>
        private DoubleData Process(DoubleData input)
        {
            var values = input.Values;
            var windowLength = (values.Length - 1) << 1;

            if (_filters == null || _sampleRate != input.SampleRate)
            {
                _sampleRate = input.SampleRate;
                BuildFilterbank(windowLength, _numberFilters, _minFreq, _maxFreq);
            }
            else if (values.Length != ((windowLength >> 1) + 1))
            {
                throw new ArgumentException("Window size is incorrect: in.length == "
                        + values.Length
                        + ", numberFftPoints == "
                        + ((windowLength >> 1) + 1));
            }

            var output = new double[_numberFilters];
            for (var i = 0; i < _numberFilters; i++)
                output[i] = _filters[i].Apply(values);

            var outputMelSpectrum = new DoubleData(output,
                    _sampleRate,
                    input.FirstSampleNumber);
            return outputMelSpectrum;
        }

        /// <summary>
        /// Reads the next Data object, which is the power spectrum of an audio input frame. Signals are returned unmodified.
        /// </summary>
        /// <returns>
        /// The next available Data or Signal object, or returns null if no Data is available.
        /// </returns>
        public override IData GetData()
        {
            var input = Predecessor.GetData();
            if (input != null)
            {
                if (input is DoubleData)
                {
                    input = Process((DoubleData)input);
                }
            }
            return input;
        }
    }
}
