using System;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.FrequencyWarp
{
    /// <summary>
    /// Filters an input power spectrum through a PLP filterbank. The filters in the filterbank are placed in the frequency
    /// axis so as to mimic the critical band, representing different perceptual effect at different frequency bands. The
    /// filter outputs are also scaled for equal loudness preemphasis. The filter shapes are defined by the {@link PLPFilter}
    /// class. Like the {@link MelFrequencyFilterBank2}, this filter bank has characteristics defined by the {@link
    /// #PROP_NUMBER_FILTERS number of filters}, the {@link #PROP_MIN_FREQ minimum frequency}, and the {@link #PROP_MAX_FREQ
    /// maximum frequency}. Unlike the {@link MelFrequencyFilterBank2}, the minimum and maximum frequencies here refer to the
    /// <b>center</b> frequencies of the filters located at the leftmost and rightmost positions, and not to the edges.
    /// Therefore, this filter bank spans a frequency range that goes beyond the limits suggested by the minimum and maximum
    /// frequencies.
    /// @author <a href="mailto:rsingh@cs.cmu.edu">rsingh</a>
    /// @version 1.0
    /// @see PLPFilter
    /// </summary>
public class PLPFrequencyFilterBank : BaseDataProcessor {

    /// <summary>
    /// The property for the number of filters in the filterbank.
    /// </summary>
    [S4Integer(DefaultValue = 32)]
    public static string PropNumberFilters = "numberFilters";

    /// <summary>
    /// The property for the center frequency of the lowest filter in the filterbank.
    /// </summary>
    [S4Double(DefaultValue = 130.0)]
    public static string PropMinFreq = "minimumFrequency";

    /// <summary>
    /// The property for the center frequency of the highest filter in the filterbank.
    /// </summary>
    [S4Double(DefaultValue = 3600.0)]
    public static string PropMaxFreq = "maximumFrequency";

    private int _sampleRate;
    private int _numberFftPoints;
    private int _numberFilters;
    private double _minFreq;
    private double _maxFreq;
    private PLPFilter[] _criticalBandFilter;
    private double[] _equalLoudnessScaling;

    public PLPFrequencyFilterBank(double minFreq, double maxFreq, int numberFilters) {
        //initLogger();
        _minFreq = minFreq;
        _maxFreq = maxFreq;
        _numberFilters = numberFilters;
    }

    public PLPFrequencyFilterBank() {
    }
  
    public override void NewProperties(PropertySheet ps) {
        base.NewProperties(ps);
        _minFreq = ps.GetDouble(PropMinFreq);
        _maxFreq = ps.GetDouble(PropMaxFreq);
        _numberFilters = ps.GetInt(PropNumberFilters);
    }

    /// <summary>
    /// Initializes this PLPFrequencyFilterBank object.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
    }


    /**
     * Build a PLP filterbank with the parameters given. The center frequencies of the PLP filters will be uniformly
     * spaced between the minimum and maximum analysis frequencies on the Bark scale. on the Bark scale.
     *
     * @throws IllegalArgumentException
     */
    private void BuildCriticalBandFilterbank()  {
        double minBarkFreq;
        double maxBarkFreq;
        double deltaBarkFreq;
        double nyquistFreq;
        double centerFreq;
        var numberDFTPoints = (_numberFftPoints >> 1) + 1;
        double[] DFTFrequencies;

        /* This is the same class of warper called by PLPFilter.java */
        var bark = new FrequencyWarper();

        _criticalBandFilter = new PLPFilter[_numberFilters];

        if (_numberFftPoints == 0) {
            throw new ArgumentException("Number of FFT points is zero");
        }
        if (_numberFilters < 1) {
            throw new ArgumentException("Number of filters illegal: "
                    + _numberFilters);
        }

        DFTFrequencies = new double[numberDFTPoints];
        nyquistFreq = _sampleRate / 2;
        for (var i = 0; i < numberDFTPoints; i++) {
            DFTFrequencies[i] = i * nyquistFreq /
                    (numberDFTPoints - 1);
        }

        /**
         * Find center frequencies of filters in the Bark scale
         * translate to linear frequency and create PLP filters
         * with these center frequencies.
         *
         * Note that minFreq and maxFreq specify the CENTER FREQUENCIES
         * of the lowest and highest PLP filters
         */


        minBarkFreq = bark.HertzToBark(_minFreq);
        maxBarkFreq = bark.HertzToBark(_maxFreq);

        if (_numberFilters < 1) {
            throw new ArgumentException("Number of filters illegal: "
                    + _numberFilters);
        }
        deltaBarkFreq = (maxBarkFreq - minBarkFreq) / (_numberFilters + 1);

        for (var i = 0; i < _numberFilters; i++) {
            centerFreq = bark.BarkToHertz(minBarkFreq + i * deltaBarkFreq);
            _criticalBandFilter[i] = new PLPFilter(DFTFrequencies, centerFreq);
        }
    }


    /**
     * This function return the equal loudness preemphasis factor at any frequency. The preemphasis function is given
     * by
     * <p/>
     * E(w) = f^4 / (f^2 + 1.6e5) ^ 2 * (f^2 + 1.44e6) / (f^2 + 9.61e6)
     * <p/>
     * This is more modern one from HTK, for some reason it's preferred over old variant, and 
     * it doesn't require conversion to radians
     * <p/>
     * E(w) = (w^2+56.8e6)*w^4/((w^2+6.3e6)^2(w^2+0.38e9)(w^6+9.58e26))
     * <p/>
     * where w is frequency in radians/second
     * @param freq
     */
    private static double LoudnessScalingFunction(double freq) {
        var fsq = freq * freq;
        var fsub = fsq / (fsq + 1.6e5);
        return fsub * fsub * ((fsq + 1.44e6) / (fsq + 9.61e6));
    }


    /// <summary>
    /// Create an array of equal loudness preemphasis scaling terms for all the filters.
    /// </summary>
    private void BuildEqualLoudnessScalingFactors() {
        double centerFreq;

        _equalLoudnessScaling = new double[_numberFilters];
        for (var i = 0; i < _numberFilters; i++) {
            centerFreq = _criticalBandFilter[i].CenterFreqInHz;
            _equalLoudnessScaling[i] = LoudnessScalingFunction(centerFreq);
        }
    }

    /// <summary>
    /// Process data, creating the power spectrum from an input audio frame.
    /// </summary>
    /// <param name="input">Input power spectrum.</param>
    /// <returns>PLP power spectrum</returns>
    /// <exception cref="System.ArgumentException">Window size is incorrect: in.length ==  + values.Length +, numberFftPoints ==  + ((_numberFftPoints >> 1) + 1)</exception>
    private DoubleData Process(DoubleData input) {

        var values = input.Values;

        if (_criticalBandFilter == null ||
                _sampleRate != input.SampleRate) {
            _numberFftPoints = (values.Length - 1) << 1;
            _sampleRate = input.SampleRate;
            BuildCriticalBandFilterbank();
            BuildEqualLoudnessScalingFactors();

        } else if (values.Length != ((_numberFftPoints >> 1) + 1)) {
            throw new ArgumentException
                    ("Window size is incorrect: in.length == " + values.Length +
                            ", numberFftPoints == " + ((_numberFftPoints >> 1) + 1));
        }

        var outputPLPSpectralArray = new double[_numberFilters];

        /**
         * Filter input power spectrum
         */
        for (var i = 0; i < _numberFilters; i++) {
            // First compute critical band filter output
            outputPLPSpectralArray[i] = _criticalBandFilter[i].FilterOutput(values);
            // Then scale it for equal loudness preemphasis
            outputPLPSpectralArray[i] *= _equalLoudnessScaling[i];
        }

        var output = new DoubleData
                (outputPLPSpectralArray, input.SampleRate,
                        input.FirstSampleNumber);

        return output;
    }

    /// <summary>
    /// Reads the next Data object, which is the power spectrum of an audio input frame. 
    /// However, it can also be other Data objects like a Signal, which is returned unmodified.
    /// </summary>
    /// <returns>
    /// The next available Data object, returns null if no Data object is available.
    /// </returns>
    public override IData GetData()  {

        var input = Predecessor.GetData();
        if (input != null) {
            if (input is DoubleData) {
                input = Process((DoubleData) input);
            }
        }

        return input;
    }
}
}
