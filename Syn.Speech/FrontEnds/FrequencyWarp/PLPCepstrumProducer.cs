using System;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.FrequencyWarp
{

    /// <summary>
    /// Computes the PLP cepstrum from a given PLP Spectrum. The power spectrum has the amplitude compressed by computing the
    /// cubed root of the PLP spectrum.  This operation is an approximation to the power law of hearing and simulates the
    /// non-linear relationship between sound intensity and perceived loudness.  Computationally, this operation is used to
    /// reduce the spectral amplitude of the critical band to enable all-pole modeling with relatively low order AR filters.
    /// The inverse discrete cosine transform (IDCT) is then applied to the autocorrelation coefficients. A linear prediction
    /// filter is then estimated from the autocorrelation values, and the linear prediction cepstrum (LPC cepstrum) is
    /// finally computed from the LP filter.
    ///  @author <a href="mailto:rsingh@cs.cmu.edu">rsingh</a>
    /// </summary>
    /// <see cref="LinearPredictor"/>
    public class PLPCepstrumProducer : BaseDataProcessor
    {

        /// <summary>
        /// The property for the number of filters in the filter bank.
        /// </summary>
        [S4Integer(DefaultValue = 32)]
        public static string PropNumberFilters = "numberFilters";

        /// <summary>
        /// The property specifying the length of the cepstrum data.
        /// </summary>
        [S4Integer(DefaultValue = 13)]
        public static string PropCepstrumLength = "cepstrumLength";

        /// <summary>
        /// The property specifying the LPC order.
        /// </summary>
        [S4Integer(DefaultValue = 14)]
        public static string PropLpcOrder = "lpcOrder";

        private int _cepstrumSize;       // size of a Cepstrum
        private int _lpcOrder;           // LPC Order to compute cepstrum
        private int _numberPlpFilters;   // number of PLP filters
        private double[][] _cosine;

        public PLPCepstrumProducer(int numberPlpFilters, int cepstrumSize, int lpcOrder)
        {
            //initLogger();
            _numberPlpFilters = numberPlpFilters;
            _cepstrumSize = cepstrumSize;
            _lpcOrder = lpcOrder;
        }

        public PLPCepstrumProducer()
        {
        }

        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            _numberPlpFilters = ps.GetInt(PropNumberFilters);
            _cepstrumSize = ps.GetInt(PropCepstrumLength);
            _lpcOrder = ps.GetInt(PropLpcOrder);
        }

        /// <summary>
        /// Constructs a PLPCepstrumProducer.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            ComputeCosine();
        }

        /// <summary>
        /// Compute the Cosine values for IDCT.
        /// </summary>
        private void ComputeCosine()
        {
            //cosine = new double[LPCOrder + 1][numberPLPFilters];
            _cosine = new double[_lpcOrder + 1][];

            var period = (double)2 * _numberPlpFilters;

            for (var i = 0; i <= _lpcOrder; i++)
            {
                var frequency = 2 * Math.PI * i / period;

                for (var j = 0; j < _numberPlpFilters; j++)
                {
                    _cosine[i][j] = Math.Cos(frequency * (j + 0.5));
                }
            }
        }

        /// <summary>
        /// Applies the intensity loudness power law. This operation is an approximation to the power law of hearing and
        /// simulates the non-linear relationship between sound intensity and percieved loudness. Computationally, this
        /// operation is used to reduce the spectral amplitude of the critical band to enable all-pole modeling with
        /// relatively low order AR filters.
        /// </summary>
        /// <param name="inspectrum">The inspectrum.</param>
        /// <returns></returns>
        private static double[] PowerLawCompress(double[] inspectrum)
        {
            var compressedspectrum = new double[inspectrum.Length];

            for (var i = 0; i < inspectrum.Length; i++)
            {
                compressedspectrum[i] = Math.Pow(inspectrum[i], 1.0 / 3.0);
            }
            return compressedspectrum;
        }

        /// <summary>
        /// Returns the next Data object, which is the PLP cepstrum of the input frame. 
        /// However, it can also be other Data objects like a EndPointSignal.
        /// </summary>
        /// <returns>
        /// The next available Data object, returns null if no Data object is available.
        /// </returns>
        public override IData GetData()
        {

            var input = Predecessor.GetData();
            var output = input;

            if (input != null)
            {
                if (input is DoubleData)
                {
                    output = Process((DoubleData)input);
                }
            }

            return output;
        }

        /// <summary>
        /// Process data, creating the PLP cepstrum from an input audio frame.
        /// </summary>
        /// <param name="input">A PLP Spectrum frame.</param>
        /// <returns>a PLP Data frame</returns>
        /// <exception cref="System.ArgumentException">PLPSpectrum size is incorrect: plpspectrum.length ==  +
        ///                             plpspectrum.Length + , numberPLPFilters ==  +
        ///                             _numberPlpFilters</exception>
        private IData Process(DoubleData input)
        {

            var plpspectrum = input.Values;

            if (plpspectrum.Length != _numberPlpFilters)
            {
                throw new ArgumentException
                        ("PLPSpectrum size is incorrect: plpspectrum.length == " +
                                plpspectrum.Length + ", numberPLPFilters == " +
                                _numberPlpFilters);
            }

            // power law compress spectrum
            var compressedspectrum = PowerLawCompress(plpspectrum);

            // compute autocorrelation values
            var autocor = ApplyCosine(compressedspectrum);

            var LPC = new LinearPredictor(_lpcOrder);
            // Compute LPC Parameters
            LPC.GetARFilter(autocor);
            // Compute LPC Cepstra
            var cepstrumDouble = LPC.GetData(_cepstrumSize);

            var cepstrum = new DoubleData
                    (cepstrumDouble, input.SampleRate,
                            input.FirstSampleNumber);

            return cepstrum;
        }


        /// <summary>
        /// Compute the discrete Cosine transform for the given power spectrum.
        /// </summary>
        /// <param name="plpspectrum">The PLPSpectrum data.</param>
        /// <returns></returns>
        private double[] ApplyCosine(double[] plpspectrum)
        {

            var autocor = new double[_lpcOrder + 1];
            double period = _numberPlpFilters;
            double beta = 0.5f;

            // apply the idct
            for (var i = 0; i <= _lpcOrder; i++)
            {

                if (_numberPlpFilters > 0)
                {
                    var cosine_i = _cosine[i];
                    var j = 0;
                    autocor[i] += (beta * plpspectrum[j] * cosine_i[j]);

                    for (j = 1; j < _numberPlpFilters; j++)
                    {
                        autocor[i] += (plpspectrum[j] * cosine_i[j]);
                    }
                    autocor[i] /= period;
                }
            }

            return autocor;
        }
    }
}
