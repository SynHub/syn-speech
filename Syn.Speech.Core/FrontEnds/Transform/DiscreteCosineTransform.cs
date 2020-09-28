using System;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Transform
{
    /// <summary>
    /// Applies a logarithm and then a Discrete Cosine Transform (DCT) to the input data. The input data is normally the mel
    /// spectrum. It has been proven that, for a sequence of real numbers, the discrete cosine transform is equivalent to the
    /// discrete Fourier transform. Therefore, this class corresponds to the last stage of converting a signal to cepstra,
    /// defined as the inverse Fourier transform of the logarithm of the Fourier transform of a signal. The property {@link
    /// #PROP_CEPSTRUM_LENGTH}refers to the dimensionality of the coefficients that are actually returned, defaulting to
    /// 13. When the input is mel-spectrum, the vector returned is the MFCC (Mel-Frequency
    /// Cepstral Coefficient) vector, where the 0-th element is the energy value.
    /// </summary>
    public class DiscreteCosineTransform : BaseDataProcessor
    {
        /// <summary>
        /// The property for the number of filters in the filterbank.
        /// </summary>
        [S4Integer(DefaultValue = 40)]
        public static string PropNumberFilters = "numberFilters";

        /// <summary>
        /// The property for the size of the cepstrum.
        /// </summary>
        [S4Integer(DefaultValue = 13)]
        public static string PropCepstrumLength = "cepstrumLength";

        protected int CepstrumSize; // size of a Cepstrum
        protected int NumberMelFilters; // number of mel-filters
        protected double[][] Melcosine;

        public DiscreteCosineTransform(int numberMelFilters, int cepstrumSize)
        {
            this.NumberMelFilters = numberMelFilters;
            this.CepstrumSize = cepstrumSize;
        }

        public DiscreteCosineTransform()
        {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);

            NumberMelFilters = ps.GetInt(PropNumberFilters);
            CepstrumSize = ps.GetInt(PropCepstrumLength);
        }

        /// <summary>
        /// 
        /// </summary>
         public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Returns the next DoubleData object, which is the mel cepstrum of the input frame. Signals are returned
        /// unmodified.
        /// </summary>
        /// <returns>the next available DoubleData melcepstrum, or Signal object, or null if no Data is available</returns>
        public override IData GetData()
        {
            var input = Predecessor.GetData(); // get the spectrum
            if (input != null && input is DoubleData)
            {
                input = Process((DoubleData)input);
            }
            return input;
        }

        private const double LogFloor = 1e-4;

        /// <summary>
        /// Process data, creating the mel cepstrum from an input spectrum frame.
        /// </summary>
        /// <param name="input">a MelSpectrum frame</param>
        /// <returns> mel Cepstrum frame</returns>
        private DoubleData Process(DoubleData input)
        {

            var melspectrum = input.Values;

            if (Melcosine == null)
            {
                NumberMelFilters = melspectrum.Length;
                ComputeMelCosine();

            }
            else if (melspectrum.Length != NumberMelFilters)
            {
                throw new ArgumentException("MelSpectrum size is incorrect: melspectrum.length == " +
                                melspectrum.Length + ", numberMelFilters == " +
                                NumberMelFilters);
            }
            // first compute the log of the spectrum
            for (var i = 0; i < melspectrum.Length; ++i)
            {
                melspectrum[i] = Math.Log(melspectrum[i] + LogFloor);
            }

            double[] cepstrum;

            // create the cepstrum by apply the melcosine filter
            cepstrum = ApplyMelCosine(melspectrum);

            return new DoubleData(cepstrum, input.SampleRate,
                    input.FirstSampleNumber);
        }

        /// <summary>
        /// Compute the MelCosine filter bank. 
        /// </summary>
        protected virtual void ComputeMelCosine()
        {
            //melcosine = new double[cepstrumSize][numberMelFilters];
            Melcosine = Java.CreateArray<double[][]>(CepstrumSize, NumberMelFilters);
            var period = (double)2 * NumberMelFilters;
            for (var i = 0; i < CepstrumSize; i++)
            {
                var frequency = 2 * Math.PI * i / period;
                for (var j = 0; j < NumberMelFilters; j++)
                {
                    Melcosine[i][j] = Math.Cos(frequency * (j + 0.5));
                }
            }
        }

        /// <summary>
        /// Apply the MelCosine filter to the given melspectrum.
        /// </summary>
        /// <param name="melspectrum">The MelSpectrum data.</param>
        /// <returns>MelCepstrum data produced by apply the MelCosine filter to the MelSpectrum data.</returns>
        protected virtual double[] ApplyMelCosine(double[] melspectrum)
        {
            // create the cepstrum
            var cepstrum = new double[CepstrumSize];
            double period = NumberMelFilters;
            var beta = 0.5;
            // apply the melcosine filter
            for (var i = 0; i < cepstrum.Length; i++)
            {
                if (NumberMelFilters > 0)
                {
                    var melcosine_i = Melcosine[i];
                    var j = 0;
                    cepstrum[i] += (beta * melspectrum[j] * melcosine_i[j]);
                    for (j = 1; j < NumberMelFilters; j++)
                    {
                        cepstrum[i] += (melspectrum[j] * melcosine_i[j]);
                    }
                    cepstrum[i] /= period;
                }
            }

            return cepstrum;
        }
    }
}
